using System.Collections.Generic;

namespace CodeShieldCore.Model;

/// <summary>
/// Exclusion rules specifying symbols that should remain un-obfuscated.
/// </summary>
public class ExclusionOptions
{
    /// <summary>
    /// List of namespace names or prefix wildcards (e.g., "MyApp.PublicContracts.*") to exclude.
    /// </summary>
    public List<string> ExcludeNamespaces { get; set; } = new();

    /// <summary>
    /// List of exact or wildcard type names (e.g., "PluginRegistry", "*Controller") to exclude.
    /// </summary>
    public List<string> ExcludeTypes { get; set; } = new();

    /// <summary>
    /// List of method names (e.g., "InitializeComponent", "ProcessCommand") to exclude.
    /// </summary>
    public List<string> ExcludeMethods { get; set; } = new();

    /// <summary>
    /// Optional regex patterns to exclude matching full type or member names.
    /// </summary>
    public List<string> ExcludeRegexPatterns { get; set; } = new();
}
