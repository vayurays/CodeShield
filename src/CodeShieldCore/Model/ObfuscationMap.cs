using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CodeShieldCore.Model;

/// <summary>
/// Contains mapping details between original symbol names and obfuscated symbol names.
/// Useful for de-obfuscating crash logs and stack traces.
/// </summary>
public class ObfuscationMap
{
    public string AssemblyName { get; set; } = string.Empty;
    public DateTime ObfuscatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type mappings: [OriginalFullName] -> [ObfuscatedFullName]
    /// </summary>
    public Dictionary<string, string> Types { get; set; } = new();

    /// <summary>
    /// Method mappings: [OriginalFullName] -> [ObfuscatedName]
    /// </summary>
    public Dictionary<string, string> Methods { get; set; } = new();

    /// <summary>
    /// Field mappings: [OriginalFullName] -> [ObfuscatedName]
    /// </summary>
    public Dictionary<string, string> Fields { get; set; } = new();

    /// <summary>
    /// Property mappings: [OriginalFullName] -> [ObfuscatedName]
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    public void SaveToFile(string filePath)
    {
        string? dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}
