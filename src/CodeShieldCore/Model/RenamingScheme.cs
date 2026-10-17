namespace CodeShieldCore.Model;

/// <summary>
/// Specifies the naming pattern used when renaming identifiers.
/// </summary>
public enum RenamingScheme
{
    /// <summary>
    /// Sequential short alphabetic characters: a, b, c, ..., z, aa, ab...
    /// Produces minimal binary size.
    /// </summary>
    ShortLetters = 0,

    /// <summary>
    /// Confusing visually similar characters like 'l', '1', 'I', 'O', '0', '_'.
    /// Extremely difficult for human reverse-engineers to read.
    /// </summary>
    Confusing = 1,

    /// <summary>
    /// Hexadecimal hash prefixed with underscore: _0x4a8f, _0x9b12...
    /// </summary>
    RandomHash = 2,

    /// <summary>
    /// Non-printable or zero-width unicode characters.
    /// Breaks many simple decompilers and text viewers.
    /// </summary>
    UnreadableUnicode = 3
}
