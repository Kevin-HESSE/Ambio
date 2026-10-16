using System.Globalization;
using System.Text;

namespace Ambio.Application.Common;

/// <summary>Matches a text typed in a search field against a text shown to the user, such as a title and a company.</summary>
public static class TextSearch
{
    /// <summary>Whether every word of <paramref name="query"/> is found in <paramref name="text"/>, ignoring case and
    /// accents: "developpeur kelvio" matches "Développeur backend · Kelvio". A query without any word matches nothing.</summary>
    public static bool Matches(string text, string query)
    {
        var words = Fold(query).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var folded = Fold(text);

        return words.Length > 0 && words.All(folded.Contains);
    }

    /// <summary>The text in lowercase, without the accents of its letters.</summary>
    private static string Fold(string text)
    {
        var folded = new StringBuilder(text.Length);

        foreach (var c in text.ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            // The accent of a decomposed letter: the letter itself is already kept.
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                folded.Append(c);
            }
        }

        return folded.ToString();
    }
}
