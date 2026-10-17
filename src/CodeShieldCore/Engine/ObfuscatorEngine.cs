using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Mono.Cecil;
using CodeShieldCore.Logging;
using CodeShieldCore.Model;
using CodeShieldCore.Protections;

namespace CodeShieldCore.Engine;

/// <summary>
/// Main orchestrator that loads the target assembly, executes protection phases,
/// writes the obfuscated output, and produces reporting/mapping artifacts.
/// </summary>
public class ObfuscatorEngine
{
    private readonly List<IProtection> _protections = new();

    public ObfuscatorEngine()
    {
        // Default pipeline
        _protections.Add(new RenamingProtection());
        _protections.Add(new StringEncryptionProtection());
    }

    /// <summary>
    /// Executes the obfuscation process based on the supplied configuration.
    /// </summary>
    public ObfuscationResult Execute(ObfuscationConfig config, IObfuscationLogger? logger = null)
    {
        logger ??= new ConsoleLogger();
        var sw = Stopwatch.StartNew();

        string inputPath = config.InputAssemblyPath;
        string outputPath = config.GetEffectiveOutputPath();
        string mapPath = config.GetEffectiveMapFilePath();

        var result = new ObfuscationResult
        {
            InputPath = inputPath,
            OutputPath = outputPath,
            MapPath = mapPath
        };

        var map = new ObfuscationMap();

        try
        {
            logger.Info("=================================================");
            logger.Info(" CodeShield .NET Obfuscator Engine v1.0");
            logger.Info("=================================================");

            if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Input assembly not found: '{inputPath}'", inputPath);
            }

            logger.Info($"Input Target:   {Path.GetFullPath(inputPath)}");
            logger.Info($"Output Target:  {Path.GetFullPath(outputPath)}");

            // Check if input is a modern .NET native AppHost executable
            bool isAppHost = AppHostHelper.IsAppHostExecutable(inputPath, out string managedDllInput);
            string effectiveManagedInput = isAppHost ? managedDllInput : inputPath;
            string effectiveManagedOutput = isAppHost && outputPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                ? Path.ChangeExtension(outputPath, ".dll")
                : outputPath;

            if (isAppHost)
            {
                logger.Info($"[AppHost] '{Path.GetFileName(inputPath)}' is a native .NET AppHost launcher.");
                logger.Info($"[AppHost] Redirecting obfuscation to managed assembly: '{Path.GetFileName(effectiveManagedInput)}'");
            }

            // Configure assembly resolver to find dependencies in the input directory
            string inputDir = Path.GetDirectoryName(Path.GetFullPath(effectiveManagedInput)) ?? AppContext.BaseDirectory;
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(inputDir);

            var readerParams = new ReaderParameters
            {
                AssemblyResolver = resolver,
                ReadWrite = false
            };

            // Check if PDB exists
            string pdbPath = Path.ChangeExtension(effectiveManagedInput, ".pdb");
            if (File.Exists(pdbPath))
            {
                try
                {
                    readerParams.ReadSymbols = true;
                }
                catch
                {
                    readerParams.ReadSymbols = false;
                }
            }

            AssemblyDefinition assembly;
            try
            {
                assembly = AssemblyDefinition.ReadAssembly(effectiveManagedInput, readerParams);
            }
            catch (BadImageFormatException)
            {
                throw new BadImageFormatException(
                    $"Format of '{Path.GetFileName(inputPath)}' is not a managed .NET assembly. " +
                    $"If targeting a .NET Core / .NET 5+ application, please select the managed assembly (.dll) " +
                    $"or ensure '{Path.ChangeExtension(Path.GetFileName(inputPath), ".dll")}' is in the same directory.");
            }

            map.AssemblyName = assembly.Name.FullName;

            var context = new ProtectionContext(assembly, config, logger, result, map);

            // Execute each protection in order
            foreach (var protection in _protections)
            {
                logger.Info($"--- Starting: {protection.Name} ---");
                bool ok = protection.Execute(context);
                if (!ok)
                {
                    throw new InvalidOperationException($"Protection {protection.Name} failed.");
                }
            }

            // Ensure destination folder exists
            string outputDir = Path.GetDirectoryName(Path.GetFullPath(effectiveManagedOutput))!;
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Write modified assembly
            var writerParams = new WriterParameters
            {
                WriteSymbols = false // Strip PDB for obfuscated builds
            };

            logger.Info($"Writing obfuscated assembly to: {effectiveManagedOutput}");
            assembly.Write(effectiveManagedOutput, writerParams);

            // If input was a native AppHost, create the patched launcher .exe
            if (isAppHost)
            {
                string targetExe = Path.ChangeExtension(outputPath, ".exe");
                string targetDllName = Path.GetFileName(effectiveManagedOutput);
                logger.Info($"Creating runnable AppHost launcher: {targetExe} -> {targetDllName}");
                AppHostHelper.CreatePatchedAppHost(inputPath, targetExe, targetDllName);
                result.OutputPath = targetExe;
            }
            else
            {
                result.OutputPath = effectiveManagedOutput;
            }

            // Copy any .runtimeconfig.json or .deps.json if present
            CopyCompanionFiles(effectiveManagedInput, effectiveManagedOutput, logger);

            // Write mapping file if enabled
            if (config.GenerateMapFile && !string.IsNullOrWhiteSpace(mapPath))
            {
                logger.Info($"Generating symbol map: {mapPath}");
                map.SaveToFile(mapPath);
            }

            sw.Stop();
            result.Elapsed = sw.Elapsed;
            result.Success = true;
            result.SymbolMap = map;

            logger.Success("=================================================");
            logger.Success($" Obfuscation completed successfully in {sw.Elapsed.TotalMilliseconds:F1}ms");
            logger.Success($" Total Symbols Renamed: {result.TotalSymbolsRenamed}");
            logger.Success($" Strings Encrypted:     {result.StringsEncrypted}");
            logger.Success("=================================================");

            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            result.Elapsed = sw.Elapsed;
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Exception = ex;

            logger.Error($"Obfuscation failed: {ex.Message}");
            logger.Debug(ex.ToString());

            return result;
        }
    }

    private static void CopyCompanionFiles(string inputPath, string outputPath, IObfuscationLogger logger)
    {
        try
        {
            string inDir = Path.GetDirectoryName(inputPath) ?? "";
            string outDir = Path.GetDirectoryName(outputPath) ?? "";

            string inBase = Path.GetFileNameWithoutExtension(inputPath);
            string outBase = Path.GetFileNameWithoutExtension(outputPath);

            CopyIfExist(Path.Combine(inDir, $"{inBase}.runtimeconfig.json"), Path.Combine(outDir, $"{outBase}.runtimeconfig.json"));
            CopyIfExist(Path.Combine(inDir, $"{inBase}.deps.json"), Path.Combine(outDir, $"{outBase}.deps.json"));
        }
        catch (Exception ex)
        {
            logger.Warning($"Could not copy companion runtime configuration files: {ex.Message}");
        }
    }

    private static void CopyIfExist(string source, string destination)
    {
        if (File.Exists(source))
        {
            File.Copy(source, destination, overwrite: true);
        }
    }
}
