using System;
using System.Collections.Generic;

namespace CodeShieldCore.Model;

/// <summary>
/// Summary report of the obfuscation execution.
/// </summary>
public class ObfuscationResult
{
    public bool Success { get; set; }
    public string InputPath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string MapPath { get; set; } = string.Empty;

    public int TypesRenamed { get; set; }
    public int MethodsRenamed { get; set; }
    public int FieldsRenamed { get; set; }
    public int PropertiesRenamed { get; set; }
    public int ParametersRenamed { get; set; }
    public int StringsEncrypted { get; set; }

    public TimeSpan Elapsed { get; set; }
    public List<string> Warnings { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public ObfuscationMap? SymbolMap { get; set; }

    public int TotalSymbolsRenamed => TypesRenamed + MethodsRenamed + FieldsRenamed + PropertiesRenamed + ParametersRenamed;
}
