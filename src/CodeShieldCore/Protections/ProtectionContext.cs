using System;
using Mono.Cecil;
using CodeShieldCore.Logging;
using CodeShieldCore.Model;

namespace CodeShieldCore.Protections;

/// <summary>
/// Execution context passed to each obfuscation protection.
/// </summary>
public class ProtectionContext
{
    public AssemblyDefinition Assembly { get; }
    public ModuleDefinition MainModule => Assembly.MainModule;
    public ObfuscationConfig Config { get; }
    public IObfuscationLogger Logger { get; }
    public ObfuscationResult Result { get; }
    public ObfuscationMap Map { get; }

    public ProtectionContext(
        AssemblyDefinition assembly,
        ObfuscationConfig config,
        IObfuscationLogger logger,
        ObfuscationResult result,
        ObfuscationMap map)
    {
        Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        Config = config ?? throw new ArgumentNullException(nameof(config));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Result = result ?? throw new ArgumentNullException(nameof(result));
        Map = map ?? throw new ArgumentNullException(nameof(map));
    }
}

/// <summary>
/// Contract for an obfuscation phase/protection.
/// </summary>
public interface IProtection
{
    string Name { get; }
    string Description { get; }
    bool Execute(ProtectionContext context);
}
