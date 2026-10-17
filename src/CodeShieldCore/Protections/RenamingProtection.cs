using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using CodeShieldCore.Logging;
using CodeShieldCore.Naming;

namespace CodeShieldCore.Protections;

/// <summary>
/// Renames types, methods, fields, properties, events, and parameters to obscure symbol names.
/// </summary>
public class RenamingProtection : IProtection
{
    public string Name => "Identifier Renaming";
    public string Description => "Renames classes, methods, fields, properties, and parameters to unreadable or compact tokens.";

    public bool Execute(ProtectionContext context)
    {
        var config = context.Config.Renaming;
        if (!config.Enabled)
        {
            context.Logger.Info("Renaming protection is disabled in configuration.");
            return true;
        }

        context.Logger.Info($"Applying {Name} using {config.Scheme} scheme...");
        INameGenerator nameGen = NameGeneratorFactory.Create(config.Scheme);

        int typesRenamed = 0;
        int methodsRenamed = 0;
        int fieldsRenamed = 0;
        int propertiesRenamed = 0;
        int parametersRenamed = 0;

        var allTypes = GetAllTypes(context.MainModule).ToList();

        // 1. Rename Types
        if (config.RenameTypes)
        {
            foreach (var type in allTypes)
            {
                if (CanRenameType(type, context))
                {
                    string oldFullName = type.FullName;
                    string newName = nameGen.NextName();
                    type.Name = newName;
                    typesRenamed++;

                    context.Map.Types[oldFullName] = type.FullName;
                    context.Logger.Debug($"Renamed Type: {oldFullName} -> {newName}");
                }
            }
        }

        // 2. Rename Members (Fields, Properties, Methods, Events, Parameters)
        foreach (var type in allTypes)
        {
            // Skip module and compiler generated helpers if excluded
            if (type.Name == "<Module>")
            {
                continue;
            }

            // Rename Fields
            if (config.RenameFields)
            {
                foreach (var field in type.Fields.ToList())
                {
                    if (CanRenameField(field, context))
                    {
                        string oldFullName = $"{field.DeclaringType.FullName}::{field.Name}";
                        string newName = nameGen.NextName();
                        field.Name = newName;
                        fieldsRenamed++;

                        context.Map.Fields[oldFullName] = newName;
                        context.Logger.Debug($"Renamed Field: {oldFullName} -> {newName}");
                    }
                }
            }

            // Rename Properties
            if (config.RenameProperties)
            {
                foreach (var prop in type.Properties.ToList())
                {
                    if (CanRenameProperty(prop, context))
                    {
                        string oldFullName = $"{prop.DeclaringType.FullName}::{prop.Name}";
                        string newName = nameGen.NextName();
                        prop.Name = newName;
                        propertiesRenamed++;

                        // Sync getter/setter names
                        if (prop.GetMethod != null && !prop.GetMethod.IsRuntimeSpecialName)
                        {
                            prop.GetMethod.Name = $"get_{newName}";
                        }
                        if (prop.SetMethod != null && !prop.SetMethod.IsRuntimeSpecialName)
                        {
                            prop.SetMethod.Name = $"set_{newName}";
                        }

                        context.Map.Properties[oldFullName] = newName;
                        context.Logger.Debug($"Renamed Property: {oldFullName} -> {newName}");
                    }
                }
            }

            // Rename Methods & Parameters
            foreach (var method in type.Methods.ToList())
            {
                if (config.RenameMethods && CanRenameMethod(method, context))
                {
                    string oldFullName = $"{method.DeclaringType.FullName}::{method.Name}";
                    string newName = nameGen.NextName();
                    method.Name = newName;
                    methodsRenamed++;

                    context.Map.Methods[oldFullName] = newName;
                    context.Logger.Debug($"Renamed Method: {oldFullName} -> {newName}");
                }

                // Rename Parameters
                if (config.RenameParameters && method.HasParameters && !method.IsRuntimeSpecialName)
                {
                    int paramIndex = 0;
                    foreach (var param in method.Parameters)
                    {
                        if (!string.IsNullOrEmpty(param.Name))
                        {
                            param.Name = $"p{paramIndex++}";
                            parametersRenamed++;
                        }
                    }
                }
            }
        }

        context.Result.TypesRenamed = typesRenamed;
        context.Result.MethodsRenamed = methodsRenamed;
        context.Result.FieldsRenamed = fieldsRenamed;
        context.Result.PropertiesRenamed = propertiesRenamed;
        context.Result.ParametersRenamed = parametersRenamed;

        context.Logger.Success($"Renamed {typesRenamed} types, {methodsRenamed} methods, {fieldsRenamed} fields, {propertiesRenamed} properties, {parametersRenamed} parameters.");
        return true;
    }

    private static IEnumerable<TypeDefinition> GetAllTypes(ModuleDefinition module)
    {
        foreach (var type in module.Types)
        {
            yield return type;
            foreach (var nested in GetAllNestedTypes(type))
            {
                yield return nested;
            }
        }
    }

    private static IEnumerable<TypeDefinition> GetAllNestedTypes(TypeDefinition type)
    {
        foreach (var nested in type.NestedTypes)
        {
            yield return nested;
            foreach (var sub in GetAllNestedTypes(nested))
            {
                yield return sub;
            }
        }
    }

    private static bool CanRenameType(TypeDefinition type, ProtectionContext context)
    {
        if (type.Name == "<Module>")
        {
            return false;
        }

        // Compiler generated types often begin with '<' or are delegates/state-machines
        if (type.Name.StartsWith("<") || type.Name.StartsWith("__"))
        {
            return false;
        }

        // Check if public API preservation is requested
        if (context.Config.Renaming.PreservePublicApi && (type.IsPublic || type.IsNestedPublic))
        {
            return false;
        }

        // Check exclusions
        if (IsExcluded(type.FullName, type.Namespace, type.Name, context))
        {
            return false;
        }

        return true;
    }

    private static bool CanRenameField(FieldDefinition field, ProtectionContext context)
    {
        if (field.IsRuntimeSpecialName || field.IsSpecialName)
        {
            return false;
        }

        // Enum value__ field
        if (field.DeclaringType.IsEnum && field.Name == "value__")
        {
            return false;
        }

        // Preserve public API if configured
        if (context.Config.Renaming.PreservePublicApi &&
            (field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly) &&
            (field.DeclaringType.IsPublic || field.DeclaringType.IsNestedPublic))
        {
            return false;
        }

        if (IsExcluded(field.DeclaringType.FullName, field.DeclaringType.Namespace, field.Name, context))
        {
            return false;
        }

        return true;
    }

    private static bool CanRenameProperty(PropertyDefinition prop, ProtectionContext context)
    {
        if (prop.IsSpecialName)
        {
            return false;
        }

        // If public API preservation is enabled
        bool isPublic = (prop.GetMethod != null && (prop.GetMethod.IsPublic || prop.GetMethod.IsFamily)) ||
                        (prop.SetMethod != null && (prop.SetMethod.IsPublic || prop.SetMethod.IsFamily));

        if (context.Config.Renaming.PreservePublicApi && isPublic &&
            (prop.DeclaringType.IsPublic || prop.DeclaringType.IsNestedPublic))
        {
            return false;
        }

        if (IsExcluded(prop.DeclaringType.FullName, prop.DeclaringType.Namespace, prop.Name, context))
        {
            return false;
        }

        return true;
    }

    private static bool CanRenameMethod(MethodDefinition method, ProtectionContext context)
    {
        // Never rename constructors or runtime special methods
        if (method.IsConstructor || method.IsRuntimeSpecialName || method.IsSpecialName)
        {
            return false;
        }

        // Never rename entry point (e.g., Main)
        if (context.Assembly.EntryPoint == method || method.Name == "Main")
        {
            return false;
        }

        // Never rename P/Invoke methods
        if (method.IsPInvokeImpl)
        {
            return false;
        }

        // Don't rename property / event accessors here (they are handled in Property/Event renaming)
        if (method.IsGetter || method.IsSetter || method.IsAddOn || method.IsRemoveOn)
        {
            return false;
        }

        // Preserve public API if configured
        if (context.Config.Renaming.PreservePublicApi &&
            (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly) &&
            (method.DeclaringType.IsPublic || method.DeclaringType.IsNestedPublic))
        {
            return false;
        }

        // Never rename overrides or implementations of methods from external/framework assemblies
        if (IsExternalOverrideOrInterface(method))
        {
            return false;
        }

        // Check exclusions
        if (IsExcluded(method.DeclaringType.FullName, method.DeclaringType.Namespace, method.Name, context))
        {
            return false;
        }

        return true;
    }

    private static bool IsExternalOverrideOrInterface(MethodDefinition method)
    {
        // Common standard object methods
        if (method.Name is "ToString" or "GetHashCode" or "Equals" or "Finalize" or "GetType" or "Dispose")
        {
            return true;
        }

        // Virtual methods that might override base or implement interface
        if (method.IsVirtual)
        {
            // If it overrides a method in a base class that is outside this assembly
            var baseType = method.DeclaringType.BaseType;
            while (baseType != null)
            {
                if (baseType.Scope.Name != method.Module.Assembly.Name.Name)
                {
                    // Base type is in another assembly (e.g. System.Object, System.Windows.Forms.Form, etc.)
                    return true;
                }
                var baseDef = baseType.Resolve();
                if (baseDef == null) break;
                baseType = baseDef.BaseType;
            }

            // Check if declaring type implements external interfaces
            foreach (var iface in method.DeclaringType.Interfaces)
            {
                if (iface.InterfaceType.Scope.Name != method.Module.Assembly.Name.Name)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsExcluded(string typeFullName, string? ns, string memberName, ProtectionContext context)
    {
        var exclusions = context.Config.Exclusions;

        // Namespace match
        if (!string.IsNullOrEmpty(ns))
        {
            foreach (var excludedNs in exclusions.ExcludeNamespaces)
            {
                if (excludedNs.EndsWith("*"))
                {
                    string prefix = excludedNs[..^1];
                    if (ns.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return true;
                }
                else if (string.Equals(ns, excludedNs, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        // Type match
        foreach (var excludedType in exclusions.ExcludeTypes)
        {
            if (excludedType.EndsWith("*"))
            {
                string prefix = excludedType[..^1];
                if (typeFullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return true;
            }
            else if (string.Equals(typeFullName, excludedType, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(typeFullName.Split('.').LastOrDefault(), excludedType, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Method / Member match
        foreach (var excludedMember in exclusions.ExcludeMethods)
        {
            if (string.Equals(memberName, excludedMember, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Regex pattern match
        foreach (var pattern in exclusions.ExcludeRegexPatterns)
        {
            try
            {
                if (Regex.IsMatch(typeFullName, pattern) || Regex.IsMatch(memberName, pattern))
                {
                    return true;
                }
            }
            catch
            {
                // Ignore malformed regex
            }
        }

        return false;
    }
}
