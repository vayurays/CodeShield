using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CodeShieldCore.Logging;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace CodeShieldCore.Protections;

/// <summary>
/// Encrypts string literals in IL method bodies and injects a runtime decryption routine.
/// </summary>
public class StringEncryptionProtection : IProtection
{
    public string Name => "String Literal Encryption";
    public string Description => "Encrypts string literals and injects a standalone runtime decryption helper into the target assembly.";

    public bool Execute(ProtectionContext context)
    {
        var config = context.Config.StringEncryption;
        if (!config.Enabled)
        {
            context.Logger.Info("String encryption is disabled in configuration.");
            return true;
        }

        context.Logger.Info($"Applying {Name}...");

        var module = context.MainModule;
        var rnd = new Random(config.KeySeed == 0 ? Environment.TickCount : config.KeySeed);

        // 1. Create and inject the decryptor helper method
        MethodDefinition decryptMethod = InjectDecrypterMethod(module);

        int stringsEncrypted = 0;

        // 2. Scan all methods in the assembly
        foreach (var type in module.Types.ToList())
        {
            stringsEncrypted += ProcessType(type, decryptMethod, config.MinStringLength, rnd, context);
        }

        context.Result.StringsEncrypted = stringsEncrypted;
        context.Logger.Success($"Encrypted {stringsEncrypted} string literals across assembly methods.");
        return true;
    }

    private static int ProcessType(TypeDefinition type, MethodDefinition decryptMethod, int minLength, Random rnd, ProtectionContext context)
    {
        int count = 0;

        // Skip our injected decrypter class
        if (type == decryptMethod.DeclaringType)
        {
            return 0;
        }

        foreach (var method in type.Methods.ToList())
        {
            if (method.HasBody && method.Body.Instructions.Count > 0)
            {
                count += ProcessMethod(method, decryptMethod, minLength, rnd, context);
            }
        }

        foreach (var nested in type.NestedTypes.ToList())
        {
            count += ProcessType(nested, decryptMethod, minLength, rnd, context);
        }

        return count;
    }

    private static int ProcessMethod(MethodDefinition method, MethodDefinition decryptMethod, int minLength, Random rnd, ProtectionContext context)
    {
        var body = method.Body;
        var il = body.GetILProcessor();
        int count = 0;

        var instructions = body.Instructions.ToList();

        for (int i = 0; i < instructions.Count; i++)
        {
            var instr = instructions[i];

            if (instr.OpCode == OpCodes.Ldstr && instr.Operand is string strVal)
            {
                if (string.IsNullOrEmpty(strVal) || strVal.Length < minLength)
                {
                    continue;
                }

                int key = rnd.Next(1000, 60000);
                string cipher = EncryptString(strVal, key);

                // Replace the string operand with cipher text
                instr.Operand = cipher;

                // Create instructions to push key and call Decrypt
                var loadKeyInstr = il.Create(OpCodes.Ldc_I4, key);
                var callDecryptInstr = il.Create(OpCodes.Call, decryptMethod);

                // Insert after the current ldstr instruction
                il.InsertAfter(instr, loadKeyInstr);
                il.InsertAfter(loadKeyInstr, callDecryptInstr);

                count++;
                context.Logger.Debug($"Encrypted string in {method.DeclaringType.Name}::{method.Name} (len: {strVal.Length})");
            }
        }

        return count;
    }

    public static string EncryptString(string plain, int key)
    {
        char[] chars = new char[plain.Length];
        for (int i = 0; i < plain.Length; i++)
        {
            chars[i] = (char)(plain[i] ^ (key + (i % 7)));
        }
        return new string(chars);
    }

    private static MethodDefinition InjectDecrypterMethod(ModuleDefinition module)
    {
        // Define internal static helper class <CodeShield_Helper>
        var helperType = new TypeDefinition(
            "",
            $"<CodeShield_Helper_{Guid.NewGuid():N}>",
            TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
            module.TypeSystem.Object);

        module.Types.Add(helperType);

        // Define: public static string Decrypt(string c, int k)
        var decryptMethod = new MethodDefinition(
            "Decrypt",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            module.TypeSystem.String);

        var pCipher = new ParameterDefinition("c", ParameterAttributes.None, module.TypeSystem.String);
        var pKey = new ParameterDefinition("k", ParameterAttributes.None, module.TypeSystem.Int32);
        decryptMethod.Parameters.Add(pCipher);
        decryptMethod.Parameters.Add(pKey);

        // Variables:
        // loc.0: char[] array
        // loc.1: int i
        var vChars = new VariableDefinition(new ArrayType(module.TypeSystem.Char));
        var vIndex = new VariableDefinition(module.TypeSystem.Int32);
        decryptMethod.Body.Variables.Add(vChars);
        decryptMethod.Body.Variables.Add(vIndex);
        decryptMethod.Body.InitLocals = true;

        var il = decryptMethod.Body.GetILProcessor();

        // Import necessary String methods
        var stringGetLength = module.ImportReference(typeof(string).GetProperty(nameof(string.Length))!.GetGetMethod()!);
        var stringGetChars = module.ImportReference(typeof(string).GetMethod("get_Chars", new[] { typeof(int) })!);
        var stringCtorChars = module.ImportReference(typeof(string).GetConstructor(new[] { typeof(char[]) })!);

        var loopCheck = il.Create(OpCodes.Nop);
        var loopStart = il.Create(OpCodes.Nop);

        // char[] vChars = new char[c.Length];
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, stringGetLength);
        il.Emit(OpCodes.Newarr, module.TypeSystem.Char);
        il.Emit(OpCodes.Stloc_0);

        // int vIndex = 0;
        il.Emit(OpCodes.Ldc_I4_0);
        il.Emit(OpCodes.Stloc_1);
        il.Emit(OpCodes.Br_S, loopCheck);

        // Loop body:
        il.Append(loopStart);
        // vChars[vIndex] = (char)(c[vIndex] ^ (k + (vIndex % 7)));
        il.Emit(OpCodes.Ldloc_0); // array
        il.Emit(OpCodes.Ldloc_1); // index

        // c[vIndex]
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldloc_1);
        il.Emit(OpCodes.Callvirt, stringGetChars);

        // k + (vIndex % 7)
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldloc_1);
        il.Emit(OpCodes.Ldc_I4_7);
        il.Emit(OpCodes.Rem);
        il.Emit(OpCodes.Add);

        // XOR & convert
        il.Emit(OpCodes.Xor);
        il.Emit(OpCodes.Conv_U2);
        il.Emit(OpCodes.Stelem_I2);

        // vIndex++
        il.Emit(OpCodes.Ldloc_1);
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Add);
        il.Emit(OpCodes.Stloc_1);

        // Loop condition:
        il.Append(loopCheck);
        il.Emit(OpCodes.Ldloc_1);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Callvirt, stringGetLength);
        il.Emit(OpCodes.Blt_S, loopStart);

        // return new string(vChars);
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Newobj, stringCtorChars);
        il.Emit(OpCodes.Ret);

        helperType.Methods.Add(decryptMethod);
        return decryptMethod;
    }
}
