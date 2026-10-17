namespace CodeShieldCore.Model;

/// <summary>
/// Configuration options for string literal encryption.
/// </summary>
public class StringEncryptionOptions
{
    /// <summary>
    /// Gets or sets whether string literal encryption is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Encryption key seed (0 for auto-generated randomized seed per build).
    /// </summary>
    public int KeySeed { get; set; } = 0;

    /// <summary>
    /// Minimum string length to encrypt. Strings shorter than this will be left as plaintext (default: 1).
    /// </summary>
    public int MinStringLength { get; set; } = 1;
}
