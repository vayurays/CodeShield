using System;
using System.Collections.Generic;
using System.IO;
using CodeShieldCore.Engine;
using CodeShieldCore.Logging;
using CodeShieldCore.Model;

namespace CodeShieldCli;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || HasFlag(args, "-h", "--help", "/?"))
        {
            PrintUsage();
            return 0;
        }

        if (HasFlag(args, "--show-msbuild"))
        {
            PrintMSBuildExample();
            return 0;
        }

        if (HasFlag(args, "--init"))
        {
            return HandleInit(args);
        }

        bool verbose = HasFlag(args, "-v", "--verbose");
        bool quiet = HasFlag(args, "-q", "--quiet");

        IObfuscationLogger logger = quiet
            ? new DelegateLogger((level, msg) =>
            {
                if (level is LogLevel.Warning or LogLevel.Error)
                {
                    Console.ForegroundColor = level == LogLevel.Error ? ConsoleColor.Red : ConsoleColor.Yellow;
                    Console.WriteLine($"[{level.ToString().ToUpperInvariant()}] {msg}");
                    Console.ResetColor();
                }
            })
            : new ConsoleLogger(verbose);

        try
        {
            // 1. Base config (optionally loaded from --config file)
            ObfuscationConfig config = new();
            string? configPath = GetOptionValue(args, "-c", "--config");
            if (!string.IsNullOrEmpty(configPath))
            {
                if (!File.Exists(configPath))
                {
                    logger.Error($"Configuration file not found: {configPath}");
                    return 1;
                }
                logger.Info($"Loading configuration from: {configPath}");
                config = ObfuscationConfig.LoadFromFile(configPath);
            }

            // 2. Command-line overrides
            string? input = GetOptionValue(args, "-i", "--input");
            if (!string.IsNullOrEmpty(input))
            {
                config.InputAssemblyPath = input;
            }

            string? output = GetOptionValue(args, "-o", "--output");
            if (!string.IsNullOrEmpty(output))
            {
                config.OutputAssemblyPath = output;
            }

            string? mapFile = GetOptionValue(args, "-m", "--map");
            if (!string.IsNullOrEmpty(mapFile))
            {
                config.MapFilePath = mapFile;
                config.GenerateMapFile = true;
            }
            if (HasFlag(args, "--no-map"))
            {
                config.GenerateMapFile = false;
            }

            // Renaming options
            if (HasFlag(args, "--no-rename"))
            {
                config.Renaming.Enabled = false;
            }
            else if (HasFlag(args, "--rename"))
            {
                config.Renaming.Enabled = true;
            }

            string? scheme = GetOptionValue(args, "--scheme");
            if (!string.IsNullOrEmpty(scheme))
            {
                config.Renaming.Scheme = scheme.ToLowerInvariant() switch
                {
                    "short" or "shortletters" or "letters" => RenamingScheme.ShortLetters,
                    "confusing" => RenamingScheme.Confusing,
                    "hash" or "randomhash" => RenamingScheme.RandomHash,
                    "unicode" or "unreadable" => RenamingScheme.UnreadableUnicode,
                    _ => throw new ArgumentException($"Invalid renaming scheme '{scheme}'. Available: short, confusing, hash, unicode")
                };
            }

            if (HasFlag(args, "--no-preserve-public"))
            {
                config.Renaming.PreservePublicApi = false;
            }
            else if (HasFlag(args, "--preserve-public"))
            {
                config.Renaming.PreservePublicApi = true;
            }

            // String encryption options
            if (HasFlag(args, "--no-encrypt-strings"))
            {
                config.StringEncryption.Enabled = false;
            }
            else if (HasFlag(args, "--encrypt-strings"))
            {
                config.StringEncryption.Enabled = true;
            }

            // Exclusions
            var excludeTypes = GetMultiOptionValues(args, "--exclude-type");
            foreach (var t in excludeTypes)
            {
                if (!config.Exclusions.ExcludeTypes.Contains(t))
                {
                    config.Exclusions.ExcludeTypes.Add(t);
                }
            }

            var excludeNs = GetMultiOptionValues(args, "--exclude-ns");
            foreach (var ns in excludeNs)
            {
                if (!config.Exclusions.ExcludeNamespaces.Contains(ns))
                {
                    config.Exclusions.ExcludeNamespaces.Add(ns);
                }
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(config.InputAssemblyPath))
            {
                logger.Error("Missing required option: --input <path> or -i <path>");
                PrintUsage();
                return 1;
            }

            if (!File.Exists(config.InputAssemblyPath))
            {
                logger.Error($"Input assembly does not exist: {config.InputAssemblyPath}");
                return 1;
            }

            // 3. Execute obfuscator engine
            var engine = new ObfuscatorEngine();
            var result = engine.Execute(config, logger);

            if (result.Success)
            {
                if (!quiet)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[SUCCESS] Build obfuscation completed without errors.");
                    Console.ResetColor();
                }
                return 0; // Success exit code for build scripts
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] Obfuscation failed: {result.ErrorMessage}");
                Console.ResetColor();
                return 2; // Obfuscation error exit code
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[FATAL] Unhandled error: {ex.Message}");
            if (verbose)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ResetColor();
            return 1;
        }
    }

    private static int HandleInit(string[] args)
    {
        string targetFile = GetOptionValue(args, "--init") ?? "codeshield.json";
        if (targetFile.StartsWith("-") || string.IsNullOrWhiteSpace(targetFile))
        {
            targetFile = "codeshield.json";
        }

        if (File.Exists(targetFile))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warning: '{targetFile}' already exists. Overwrite? (y/N)");
            Console.ResetColor();
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.KeyChar != 'y' && key.KeyChar != 'Y')
            {
                Console.WriteLine("Initialization aborted.");
                return 0;
            }
        }

        var templateConfig = new ObfuscationConfig
        {
            InputAssemblyPath = "bin/Release/net10.0/MyProject.dll",
            OutputAssemblyPath = "bin/Release/net10.0/MyProject.Obfuscated.dll",
            GenerateMapFile = true,
            Renaming = new RenamingOptions
            {
                Enabled = true,
                Scheme = RenamingScheme.ShortLetters,
                PreservePublicApi = true,
                RenameTypes = true,
                RenameMethods = true,
                RenameFields = true,
                RenameProperties = true,
                RenameParameters = true
            },
            StringEncryption = new StringEncryptionOptions
            {
                Enabled = true,
                KeySeed = 0,
                MinStringLength = 1
            },
            Exclusions = new ExclusionOptions
            {
                ExcludeNamespaces = { "MyProject.Contracts.*" },
                ExcludeTypes = { "KeepThisClass" },
                ExcludeMethods = { "SpecialHookMethod" }
            }
        };

        templateConfig.SaveToFile(targetFile);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Successfully generated sample configuration template: {Path.GetFullPath(targetFile)}");
        Console.ResetColor();
        return 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine(@"
CodeShield CLI - .NET Assembly Obfuscator
Usage: codeshield [options]

Required:
  -i, --input <path>            Path to input .NET assembly (.dll / .exe)

Output:
  -o, --output <path>           Path for obfuscated assembly (default: [input].Obfuscated.dll)
  -m, --map <path>              Path for symbol map file (default: [output].map.json)
  --no-map                      Disable generation of symbol mapping file

Configuration:
  -c, --config <path>           Path to .codeshield or .json configuration file
  --init [path]                 Generate a starter configuration file template

Protections:
  --rename                      Enable identifier renaming (default)
  --no-rename                   Disable identifier renaming
  --scheme <name>               Renaming scheme: short | confusing | hash | unicode (default: short)
  --preserve-public             Preserve public classes and members (default: true)
  --no-preserve-public          Obfuscate all symbols including public ones (ideal for apps/exes)

  --encrypt-strings             Enable string literal encryption (default)
  --no-encrypt-strings          Disable string encryption

Exclusions:
  --exclude-type <name>         Exclude type name or wildcard pattern (repeatable)
  --exclude-ns <namespace>      Exclude namespace or wildcard pattern (repeatable)

Diagnostics & CI:
  -v, --verbose                 Display detailed diagnostic logs
  -q, --quiet                   Minimal console output (only warnings/errors)
  --show-msbuild                Show example MSBuild target configuration
  -h, --help                    Show this help message

Exit Codes:
  0 = Obfuscation succeeded (build script should proceed)
  1 = Invalid arguments or file not found
  2 = Obfuscation engine error
");
    }

    private static void PrintMSBuildExample()
    {
        Console.WriteLine(@"
========================================================================
 MSBuild Integration Example
 Add this Target to your .csproj to run CodeShield automatically in Release builds:
========================================================================

<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <!-- Run CodeShield after compilation in Release mode -->
  <Target Name=""ObfuscateWithCodeShield"" AfterTargets=""AfterBuild"" Condition=""'$(Configuration)' == 'Release'"">
    <Message Importance=""High"" Text=""[CodeShield] Obfuscating $(TargetPath)..."" />
    <Exec Command=""codeshield -i &quot;$(TargetPath)&quot; -o &quot;$(TargetPath)&quot; --encrypt-strings --no-preserve-public""
          StandardOutputImportance=""Low""
          StandardErrorImportance=""High"" />
    <Message Importance=""High"" Text=""[CodeShield] Obfuscation complete!"" />
  </Target>
</Project>
========================================================================
");
    }

    private static bool HasFlag(string[] args, params string[] flags)
    {
        foreach (var arg in args)
        {
            foreach (var flag in flags)
            {
                if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static string? GetOptionValue(string[] args, params string[] optionNames)
    {
        for (int i = 0; i < args.Length; i++)
        {
            foreach (var opt in optionNames)
            {
                if (string.Equals(args[i], opt, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
                if (args[i].StartsWith(opt + "=", StringComparison.OrdinalIgnoreCase))
                {
                    return args[i][(opt.Length + 1)..];
                }
            }
        }
        return null;
    }

    private static List<string> GetMultiOptionValues(string[] args, string optionName)
    {
        var list = new List<string>();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], optionName, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                list.Add(args[i + 1]);
            }
            else if (args[i].StartsWith(optionName + "=", StringComparison.OrdinalIgnoreCase))
            {
                list.Add(args[i][(optionName.Length + 1)..]);
            }
        }
        return list;
    }
}
