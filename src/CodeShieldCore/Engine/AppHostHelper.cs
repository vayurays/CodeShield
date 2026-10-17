using System;
using System.IO;
using System.Text;
using Mono.Cecil;

namespace CodeShieldCore.Engine;

/// <summary>
/// Handles modern .NET (Core / 5 / 6 / 7 / 8 / 9 / 10) native AppHost executables.
/// In modern .NET, MyApp.exe is a native unmanaged host stub, while the actual managed
/// IL code resides in the companion MyApp.dll.
/// </summary>
public static class AppHostHelper
{
    /// <summary>
    /// Checks whether the specified file is a native .NET AppHost launcher executable.
    /// If true, outputs the path to the companion managed .dll assembly.
    /// </summary>
    public static bool IsAppHostExecutable(string exePath, out string managedDllPath)
    {
        managedDllPath = string.Empty;
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
        {
            return false;
        }

        if (!string.Equals(Path.GetExtension(exePath), ".exe", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string candidateDll = Path.ChangeExtension(exePath, ".dll");
        if (!File.Exists(candidateDll))
        {
            return false;
        }

        // Test if the .exe can be read by Mono.Cecil.
        // If it throws BadImageFormatException, it is a native binary without a CLR header (modern .NET AppHost).
        try
        {
            using var asm = AssemblyDefinition.ReadAssembly(exePath);
            return false; // Valid CLR .exe (.NET Framework)
        }
        catch (BadImageFormatException)
        {
            managedDllPath = candidateDll;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Patches a copied native AppHost executable to point to the newly obfuscated .dll.
    /// Modern .NET AppHost executables contain a 1024-byte placeholder buffer with the target DLL name.
    /// </summary>
    public static bool CreatePatchedAppHost(string sourceAppHostExe, string destinationAppHostExe, string targetDllFileName)
    {
        try
        {
            if (!File.Exists(sourceAppHostExe))
            {
                return false;
            }

            string sourceDllName = Path.GetFileNameWithoutExtension(sourceAppHostExe) + ".dll";
            byte[] bytes = File.ReadAllBytes(sourceAppHostExe);
            byte[] oldTarget = Encoding.UTF8.GetBytes(sourceDllName);

            int offset = FindBytes(bytes, oldTarget);
            if (offset >= 0)
            {
                byte[] newTarget = Encoding.UTF8.GetBytes(targetDllFileName + "\0");
                if (newTarget.Length < 1000)
                {
                    Array.Copy(newTarget, 0, bytes, offset, newTarget.Length);
                    // Clear trailing characters from old target name if any
                    for (int i = newTarget.Length; i < oldTarget.Length; i++)
                    {
                        bytes[offset + i] = 0;
                    }
                }
            }

            string? destDir = Path.GetDirectoryName(destinationAppHostExe);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.WriteAllBytes(destinationAppHostExe, bytes);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static int FindBytes(byte[] haystack, byte[] needle)
    {
        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
            {
                return i;
            }
        }
        return -1;
    }
}
