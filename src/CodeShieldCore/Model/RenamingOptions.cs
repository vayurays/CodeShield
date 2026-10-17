namespace CodeShieldCore.Model;

/// <summary>
/// Configuration options for symbol renaming protection.
/// </summary>
public class RenamingOptions
{
    /// <summary>
    /// Gets or sets whether identifier renaming is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the scheme used to generate new names.
    /// </summary>
    public RenamingScheme Scheme { get; set; } = RenamingScheme.ShortLetters;

    /// <summary>
    /// Gets or sets whether public types, methods, and members should be preserved.
    /// Recommended for class libraries where public API must remain consumable.
    /// Can be disabled for standalone executables.
    /// </summary>
    public bool PreservePublicApi { get; set; } = true;

    /// <summary>
    /// Gets or sets whether type (class, struct, interface, enum) names should be renamed.
    /// </summary>
    public bool RenameTypes { get; set; } = true;

    /// <summary>
    /// Gets or sets whether method names should be renamed.
    /// </summary>
    public bool RenameMethods { get; set; } = true;

    /// <summary>
    /// Gets or sets whether field names should be renamed.
    /// </summary>
    public bool RenameFields { get; set; } = true;

    /// <summary>
    /// Gets or sets whether property names should be renamed.
    /// </summary>
    public bool RenameProperties { get; set; } = true;

    /// <summary>
    /// Gets or sets whether event names should be renamed.
    /// </summary>
    public bool RenameEvents { get; set; } = true;

    /// <summary>
    /// Gets or sets whether method parameter names should be renamed.
    /// </summary>
    public bool RenameParameters { get; set; } = true;
}
