using NanoidDotNet;

namespace Ambio.Infrastructure.Common;

/// <summary>The alphabet and size of the short ids in URLs (ADR 0014), from the <see cref="Section"/> section.</summary>
public sealed class ShortIdOptions
{
    public const string Section = "Ambio:ShortId";

    /// <summary>Numeric for now (10⁶ ids). It can move to lowercase letters and digits without a migration, but
    /// never contains <c>-</c>, which separates the short id from the slug.</summary>
    public string Alphabet { get; set; } = Nanoid.Alphabets.Digits;

    public int Size { get; set; } = 6;

    /// <summary>A non-empty lowercase alphabet without <c>-</c>, and a positive size.</summary>
    public bool IsValid() =>
        Alphabet.Length > 0
        && !Alphabet.Contains('-')
        && Alphabet == Alphabet.ToLowerInvariant()
        && Size > 0;
}
