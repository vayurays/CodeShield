using System;
using System.Collections.Generic;
using System.Text;
using CodeShieldCore.Model;

namespace CodeShieldCore.Naming;

/// <summary>
/// Interface for obfuscated identifier name generation.
/// </summary>
public interface INameGenerator
{
    string NextName();
    void Reset();
}

/// <summary>
/// Generates sequential alphabetic identifiers: a, b, c, ... z, aa, ab, ...
/// </summary>
public class ShortLettersNameGenerator : INameGenerator
{
    private int _counter;
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyz";

    public ShortLettersNameGenerator(int start = 0)
    {
        _counter = start;
    }

    public string NextName()
    {
        int n = _counter++;
        var sb = new StringBuilder();

        do
        {
            sb.Insert(0, Alphabet[n % Alphabet.Length]);
            n = (n / Alphabet.Length) - 1;
        } while (n >= 0);

        return sb.ToString();
    }

    public void Reset()
    {
        _counter = 0;
    }
}

/// <summary>
/// Generates confusing visual tokens combining 'I', 'l', '1', 'O', '0', and '_'.
/// </summary>
public class ConfusingNameGenerator : INameGenerator
{
    private int _counter;
    private static readonly char[] ConfusingChars = ['I', 'l', '1', 'O', '0', '_'];
    private readonly HashSet<string> _generated = new();

    public string NextName()
    {
        while (true)
        {
            int n = _counter++;
            var sb = new StringBuilder();
            
            // Prefix with an alphabetic or underscore character so it's a valid CLR identifier
            sb.Append(n % 2 == 0 ? '_' : 'I');
            
            int temp = n;
            do
            {
                sb.Append(ConfusingChars[temp % ConfusingChars.Length]);
                temp /= ConfusingChars.Length;
            } while (temp > 0);

            string candidate = sb.ToString();
            if (_generated.Add(candidate))
            {
                return candidate;
            }
        }
    }

    public void Reset()
    {
        _counter = 0;
        _generated.Clear();
    }
}

/// <summary>
/// Generates hex-hash identifiers: _0x4f1a, _0x8b2e, etc.
/// </summary>
public class RandomHashNameGenerator : INameGenerator
{
    private int _counter;
    private readonly Random _random;

    public RandomHashNameGenerator(int seed = 1337)
    {
        _random = new Random(seed);
        _counter = 0;
    }

    public string NextName()
    {
        _counter++;
        uint val = (uint)(_counter ^ _random.Next(0, int.MaxValue));
        return $"_0x{val:x8}";
    }

    public void Reset()
    {
        _counter = 0;
    }
}

/// <summary>
/// Generates identifiers with invisible or confusing Unicode characters.
/// </summary>
public class UnreadableUnicodeNameGenerator : INameGenerator
{
    private int _counter;
    // Uses invisible zero-width spaces and zero-width joiners
    private static readonly char[] InvisibleChars = ['\u200B', '\u200C', '\u200D', '\u2060'];

    public string NextName()
    {
        int n = _counter++;
        var sb = new StringBuilder();
        // Leading valid letter so runtime and loader accepts the name
        sb.Append('\u00F8'); // ø

        int temp = n;
        do
        {
            sb.Append(InvisibleChars[temp % InvisibleChars.Length]);
            temp /= InvisibleChars.Length;
        } while (temp > 0);

        return sb.ToString();
    }

    public void Reset()
    {
        _counter = 0;
    }
}

/// <summary>
/// Factory to instantiate name generators based on scheme.
/// </summary>
public static class NameGeneratorFactory
{
    public static INameGenerator Create(RenamingScheme scheme, int seed = 0)
    {
        return scheme switch
        {
            RenamingScheme.ShortLetters => new ShortLettersNameGenerator(),
            RenamingScheme.Confusing => new ConfusingNameGenerator(),
            RenamingScheme.RandomHash => new RandomHashNameGenerator(seed == 0 ? Environment.TickCount : seed),
            RenamingScheme.UnreadableUnicode => new UnreadableUnicodeNameGenerator(),
            _ => new ShortLettersNameGenerator()
        };
    }
}
