using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CodeShieldCore.Engine;
using CodeShieldCore.Logging;
using CodeShieldCore.Model;
using CodeShieldCore.Naming;
using Xunit;

namespace CodeShieldTests;

public class ObfuscationTests
{
    [Fact]
    public void Test_NameGenerators_ProduceUniqueNames()
    {
        var schemes = new[]
        {
            RenamingScheme.ShortLetters,
            RenamingScheme.Confusing,
            RenamingScheme.RandomHash,
            RenamingScheme.UnreadableUnicode
        };

        foreach (var scheme in schemes)
        {
            var gen = NameGeneratorFactory.Create(scheme);
            var names = Enumerable.Range(0, 100).Select(_ => gen.NextName()).ToList();
            Assert.Equal(100, names.Distinct().Count());
        }
    }

    [Fact]
    public void Test_Config_SaveAndLoad_RoundTrip()
    {
        var original = new ObfuscationConfig
        {
            InputAssemblyPath = "sample.dll",
            OutputAssemblyPath = "sample_out.dll",
            GenerateMapFile = true,
            Renaming = new RenamingOptions
            {
                Enabled = true,
                Scheme = RenamingScheme.Confusing,
                PreservePublicApi = false,
                RenameTypes = true,
                RenameMethods = true
            },
            StringEncryption = new StringEncryptionOptions
            {
                Enabled = true,
                KeySeed = 42,
                MinStringLength = 2
            },
            Exclusions = new ExclusionOptions
            {
                ExcludeTypes = { "KeepMe" },
                ExcludeNamespaces = { "System.*" }
            }
        };

        string tempPath = Path.Combine(Path.GetTempPath(), $"codeshield_test_{Guid.NewGuid():N}.json");
        try
        {
            original.SaveToFile(tempPath);
            Assert.True(File.Exists(tempPath));

            var loaded = ObfuscationConfig.LoadFromFile(tempPath);
            Assert.Equal(original.InputAssemblyPath, loaded.InputAssemblyPath);
            Assert.Equal(original.Renaming.Scheme, loaded.Renaming.Scheme);
            Assert.Equal(original.Renaming.PreservePublicApi, loaded.Renaming.PreservePublicApi);
            Assert.Equal(original.StringEncryption.KeySeed, loaded.StringEncryption.KeySeed);
            Assert.Contains("KeepMe", loaded.Exclusions.ExcludeTypes);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void Test_ObfuscateSampleApp_HidesStrings_AndExecutesSuccessfully()
    {
        // 1. Locate the built SampleApp.dll
        string currentDir = AppContext.BaseDirectory;
        // Search upwards for samples/CodeShieldSampleApp/bin
        string solutionRoot = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", ".."));
        string sampleBinDir = Path.Combine(solutionRoot, "samples", "CodeShieldSampleApp", "bin", "Debug");
        string sampleDll = Path.Combine(sampleBinDir, "net10.0", "CodeShieldSampleApp.dll");
        if (!File.Exists(sampleDll))
        {
            sampleDll = Directory.Exists(sampleBinDir)
                ? Directory.GetFiles(sampleBinDir, "CodeShieldSampleApp.dll", SearchOption.AllDirectories).FirstOrDefault() ?? sampleDll
                : sampleDll;
        }

        if (!File.Exists(sampleDll))
        {
            // Build it if not already built
            var psi = new ProcessStartInfo("dotnet", $"build \"{Path.Combine(solutionRoot, "samples", "CodeShieldSampleApp", "CodeShieldSampleApp.csproj")}\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            Process.Start(psi)!.WaitForExit();
            
            if (!File.Exists(sampleDll))
            {
                sampleDll = Directory.GetFiles(sampleBinDir, "CodeShieldSampleApp.dll", SearchOption.AllDirectories).FirstOrDefault() ?? sampleDll;
            }
        }

        Assert.True(File.Exists(sampleDll), $"SampleApp DLL was not found at: {sampleDll}");

        // 2. Setup obfuscation config
        string outputDll = Path.Combine(Path.GetDirectoryName(sampleDll)!, "CodeShieldSampleApp.Obfuscated.dll");
        string mapFile = Path.Combine(Path.GetDirectoryName(sampleDll)!, "CodeShieldSampleApp.map.json");

        var config = new ObfuscationConfig
        {
            InputAssemblyPath = sampleDll,
            OutputAssemblyPath = outputDll,
            MapFilePath = mapFile,
            GenerateMapFile = true,
            Renaming = new RenamingOptions
            {
                Enabled = true,
                Scheme = RenamingScheme.ShortLetters,
                PreservePublicApi = false, // Obfuscate all classes since it's an executable
                RenameTypes = true,
                RenameMethods = true,
                RenameFields = true,
                RenameProperties = true,
                RenameParameters = true
            },
            StringEncryption = new StringEncryptionOptions
            {
                Enabled = true,
                MinStringLength = 1
            }
        };

        var engine = new ObfuscatorEngine();
        var logger = new ConsoleLogger(verbose: true);
        var result = engine.Execute(config, logger);

        // 3. Verify execution result
        Assert.True(result.Success, $"Obfuscation failed: {result.ErrorMessage}");
        Assert.True(File.Exists(outputDll), "Obfuscated DLL was not generated.");
        Assert.True(File.Exists(mapFile), "Symbol map file was not generated.");
        Assert.True(result.StringsEncrypted > 0, "No strings were encrypted.");
        Assert.True(result.TotalSymbolsRenamed > 0, "No symbols were renamed.");

        // 4. Verify that plaintext strings are absent from ldstr instructions in the output assembly
        using (var asm = AssemblyDefinition.ReadAssembly(outputDll))
        {
            var stringsInAssembly = asm.MainModule.Types
                .SelectMany(t => t.Methods)
                .Where(m => m.HasBody)
                .SelectMany(m => m.Body.Instructions)
                .Where(i => i.OpCode == OpCodes.Ldstr)
                .Select(i => (string)i.Operand)
                .ToList();

            // None of these sensitive strings should appear in plaintext
            Assert.DoesNotContain("RootAdminKey_2026_Secure", stringsInAssembly);
            Assert.DoesNotContain("sk_live_920491823091283091283", stringsInAssembly);
            Assert.DoesNotContain("https://api.payments.internal/v2/checkout", stringsInAssembly);
        }

        // 5. Execute the obfuscated assembly with dotnet and verify runtime correctness!
        var runPsi = new ProcessStartInfo("dotnet", $"\"{outputDll}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(runPsi)!;
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.Equal(0, process.ExitCode);
        Assert.Contains("All tests completed: Vault Unlocked: True, Payment: True, License: True", stdout);
        Assert.Contains("[Vault] Successfully unlocked with secret key.", stdout);
    }

    [Fact]
    public void Test_ObfuscateAppHostExe_Directly_RedirectsToDll_AndPatchesExe()
    {
        string currentDir = AppContext.BaseDirectory;
        string solutionRoot = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", ".."));
        string sampleBinDir = Path.Combine(solutionRoot, "samples", "CodeShieldSampleApp", "bin", "Debug", "net10.0");
        string sampleExe = Path.Combine(sampleBinDir, "CodeShieldSampleApp.exe");

        if (!File.Exists(sampleExe))
        {
            var psi = new ProcessStartInfo("dotnet", $"build \"{Path.Combine(solutionRoot, "samples", "CodeShieldSampleApp", "CodeShieldSampleApp.csproj")}\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            Process.Start(psi)!.WaitForExit();
        }

        Assert.True(File.Exists(sampleExe), $"SampleApp EXE was not found at: {sampleExe}");

        string outputExe = Path.Combine(sampleBinDir, "CodeShieldSampleApp.AppHostTest.exe");
        string outputDll = Path.Combine(sampleBinDir, "CodeShieldSampleApp.AppHostTest.dll");

        var config = new ObfuscationConfig
        {
            InputAssemblyPath = sampleExe,
            OutputAssemblyPath = outputExe,
            Renaming = new RenamingOptions
            {
                Enabled = true,
                Scheme = RenamingScheme.ShortLetters,
                PreservePublicApi = false
            },
            StringEncryption = new StringEncryptionOptions
            {
                Enabled = true
            }
        };

        var engine = new ObfuscatorEngine();
        var result = engine.Execute(config, new ConsoleLogger(verbose: true));

        Assert.True(result.Success, $"Obfuscation failed: {result.ErrorMessage}");
        Assert.True(File.Exists(outputDll), "Obfuscated DLL was not generated from AppHost EXE.");
        Assert.True(File.Exists(outputExe), "Patched AppHost EXE was not generated.");

        // Execute the patched EXE directly!
        var runPsi = new ProcessStartInfo(outputExe)
        {
            WorkingDirectory = sampleBinDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(runPsi)!;
        string stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Assert.Equal(0, process.ExitCode);
        Assert.Contains("All tests completed", stdout);
    }
}
