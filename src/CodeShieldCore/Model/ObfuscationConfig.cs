using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeShieldCore.Model;

/// <summary>
/// Root configuration defining target files, protections, exclusions, and output options.
/// Can be saved/loaded as a .codeshield or .json file for CLI build scripts and WinForms UI.
/// </summary>
public class ObfuscationConfig
{
    /// <summary>
    /// Path to the input .NET assembly (.dll or .exe).
    /// </summary>
    public string InputAssemblyPath { get; set; } = string.Empty;

    /// <summary>
    /// Path where the obfuscated assembly should be written.
    /// If null or empty, defaults to appending ".Obfuscated" to the input file name.
    /// </summary>
    public string OutputAssemblyPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether to generate a JSON symbol map file mapping original names to obfuscated names.
    /// </summary>
    public bool GenerateMapFile { get; set; } = true;

    /// <summary>
    /// Explicit path for the symbol map file. If empty, defaults to [OutputAssemblyPath].map.json.
    /// </summary>
    public string MapFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Renaming protection options.
    /// </summary>
    public RenamingOptions Renaming { get; set; } = new();

    /// <summary>
    /// String literal encryption options.
    /// </summary>
    public StringEncryptionOptions StringEncryption { get; set; } = new();

    /// <summary>
    /// Exclusion rules for types, namespaces, and methods.
    /// </summary>
    public ExclusionOptions Exclusions { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Saves the configuration to a JSON file.
    /// </summary>
    public void SaveToFile(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads a configuration from a JSON file.
    /// </summary>
    public static ObfuscationConfig LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}", filePath);
        }

        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<ObfuscationConfig>(json, JsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize configuration file.");
    }

    /// <summary>
    /// Resolves the effective output assembly path based on the input path if not explicitly provided.
    /// </summary>
    public string GetEffectiveOutputPath()
    {
        if (!string.IsNullOrWhiteSpace(OutputAssemblyPath))
        {
            return OutputAssemblyPath;
        }

        if (string.IsNullOrWhiteSpace(InputAssemblyPath))
        {
            return string.Empty;
        }

        string dir = Path.GetDirectoryName(InputAssemblyPath) ?? string.Empty;
        string name = Path.GetFileNameWithoutExtension(InputAssemblyPath);
        string ext = Path.GetExtension(InputAssemblyPath);

        return Path.Combine(dir, $"{name}.Obfuscated{ext}");
    }

    /// <summary>
    /// Resolves the effective map file path.
    /// </summary>
    public string GetEffectiveMapFilePath()
    {
        if (!string.IsNullOrWhiteSpace(MapFilePath))
        {
            return MapFilePath;
        }

        string outputPath = GetEffectiveOutputPath();
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return string.Empty;
        }

        return $"{outputPath}.map.json";
    }
}
