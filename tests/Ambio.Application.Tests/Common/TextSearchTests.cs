using Ambio.Application.Common;

namespace Ambio.Application.Tests.Common;

public class TextSearchTests
{
    [Theory]
    [MemberData(nameof(MatchingQueries))]
    public void Matches_WithEveryWordFound_ReturnsTrue(string text, string query)
    {
        Assert.True(TextSearch.Matches(text, query));
    }

    [Theory]
    [MemberData(nameof(NonMatchingQueries))]
    public void Matches_WithAWordMissingOrNoWord_ReturnsFalse(string text, string query)
    {
        Assert.False(TextSearch.Matches(text, query));
    }

    public static TheoryData<string, string> MatchingQueries => new()
    {
        { "Data Engineer Lumen Health", "data" },
        { "Data Engineer Lumen Health", "ENGIN" },
        { "Data Engineer Lumen Health", "lumen engineer" },
        { "Développeur backend Kelvio", "developpeur" },
        { "Developpeur backend Kelvio", "Développeur" },
        { "Full-stack Engineer Kelvio", " full-stack  kelvio " },
    };

    public static TheoryData<string, string> NonMatchingQueries => new()
    {
        { "Data Engineer Lumen Health", "frontend" },
        { "Data Engineer Lumen Health", "data frontend" },
        { "Data Engineer Lumen Health", "" },
        { "Data Engineer Lumen Health", " \t" },
    };
}
