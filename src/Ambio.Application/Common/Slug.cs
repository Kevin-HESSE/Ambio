using System.Globalization;
using System.Text;

namespace Ambio.Application.Common;

/// <summary>Builds the readable part of a URL from a text, such as a company name.</summary>
public static class Slug
{
    public const int MaxLength = 60;

    /// <summary>The text in lowercase ASCII: accents removed, œ, æ and ß spelled out, and every run of other
    /// characters replaced by one <c>-</c>, cut to <see cref="MaxLength"/> characters. Returns
    /// <paramref name="fallback"/> when no letter or digit is left.</summary>
    public static string From(string text, string fallback)
    {
        var slug = new StringBuilder(text.Length);

        foreach (var c in text.ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            switch (c)
            {
                case >= 'a' and <= 'z' or >= '0' and <= '9':
                    slug.Append(c);
                    break;
                case 'œ':
                    slug.Append("oe");
                    break;
                case 'æ':
                    slug.Append("ae");
                    break;
                case 'ß':
                    slug.Append("ss");
                    break;
                case var _ when CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark:
                    // The accent of a decomposed letter: the letter itself is already kept.
                    break;
                default:
                    if (slug.Length > 0 && slug[^1] != '-')
                    {
                        slug.Append('-');
                    }
                    break;
            }
        }

        var result = slug.ToString(0, Math.Min(slug.Length, MaxLength)).Trim('-');
        return result.Length > 0 ? result : fallback;
    }
}
