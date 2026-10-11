using Ambio.Application.Common;

namespace Ambio.Application.Tests.Common;

public class SlugTests
{
    private const string Fallback = "company";

    [Theory]
    [MemberData(nameof(Texts))]
    public void From_WithLettersOrDigits_ReturnsLowercaseAsciiWithDashes(string text, string expected)
    {
        Assert.Equal(expected, Slug.From(text, Fallback));
    }

    [Theory]
    [MemberData(nameof(TextsWithoutLetterOrDigit))]
    public void From_WithoutLetterOrDigitLeft_ReturnsFallback(string text)
    {
        Assert.Equal(Fallback, Slug.From(text, Fallback));
    }

    [Fact]
    public void From_WithLongText_CutsToMaxLength()
    {
        var slug = Slug.From(new string('a', Slug.MaxLength + 10), Fallback);

        Assert.Equal(new string('a', Slug.MaxLength), slug);
    }

    [Fact]
    public void From_WhenCutOnSeparator_DropsTrailingDash()
    {
        var slug = Slug.From($"{new string('a', Slug.MaxLength - 1)} bcd", Fallback);

        Assert.Equal(new string('a', Slug.MaxLength - 1), slug);
    }

    public static TheoryData<string, string> Texts => new()
    {
        { "Brightwave", "brightwave" },
        { "Acme Inc.", "acme-inc" },
        { "Acme, Inc", "acme-inc" },
        { "Société Générale", "societe-generale" },
        { "Cœur de Pirate", "coeur-de-pirate" },
        { "Æther Straße", "aether-strasse" },
        { "  --L'Oréal--  ", "l-oreal" },
        { "Ørsted", "rsted" },
        { "C++ & .NET 10", "c-net-10" },
    };

    public static TheoryData<string> TextsWithoutLetterOrDigit =>
    [
        "",
        "!!!",
        "Яндекс",
    ];
}
