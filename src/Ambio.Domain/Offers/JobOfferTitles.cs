using System.Globalization;
using System.Text;

namespace Ambio.Domain.Offers;

/// <summary>Compares offer titles, to suggest an existing offer instead of creating a duplicate.</summary>
public static class JobOfferTitles
{
    /// <summary>Whether the two titles share at least half of all their words. Words are compared ignoring case,
    /// accents and punctuation: "Full-stack Engineer" and "full stack engineer" are similar, "Backend Developer"
    /// and "Frontend Developer" aren't. A title without any letter or digit is similar to none.</summary>
    public static bool AreSimilar(string title, string otherTitle)
    {
        var words = Words(title);
        var otherWords = Words(otherTitle);

        if (words.Count == 0 || otherWords.Count == 0)
        {
            return false;
        }

        var common = words.Count(otherWords.Contains);
        var all = words.Count + otherWords.Count - common;
        return 2 * common >= all;
    }

    /// <summary>The words of the title in lowercase, without accents: every run of characters other than letters
    /// and digits separates two words.</summary>
    private static HashSet<string> Words(string title)
    {
        var text = new StringBuilder(title.Length);

        foreach (var c in title.ToLowerInvariant().Normalize(NormalizationForm.FormD))
        {
            if (char.IsLetterOrDigit(c))
            {
                text.Append(c);
            }
            else if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                // The accent of a decomposed letter is dropped; anything else ends the word.
                text.Append(' ');
            }
        }

        return [.. text.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)];
    }
}
