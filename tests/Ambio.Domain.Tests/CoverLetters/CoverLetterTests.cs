using Ambio.Domain.CoverLetters;

namespace Ambio.Domain.Tests.CoverLetters;

public class CoverLetterTests
{
    private const string Title = "Cover letter · Brightwave";
    private const string Content = "Dear Brightwave team,\n\nI am applying for the Backend Developer position.";

    [Fact]
    public void New_GeneratesVersion7Id()
    {
        Assert.Equal(7, NewLetter().Id.Version);
    }

    [Fact]
    public void Title_WithSurroundingWhitespace_IsTrimmed()
    {
        var letter = NewLetter(title: $"  {Title}\t");

        Assert.Equal(Title, letter.Title);
    }

    [Fact]
    public void Title_WhenNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => NewLetter(title: null!));
    }

    [Theory]
    [MemberData(nameof(BlankValues))]
    public void Title_WhenBlank_ThrowsArgumentException(string title)
    {
        Assert.Throws<ArgumentException>(() => NewLetter(title: title));
    }

    [Fact]
    public void Content_WithSurroundingBlankLines_IsTrimmedAndKeepsItsLineBreaks()
    {
        var letter = NewLetter(content: $"\n\n{Content}\n  ");

        Assert.Equal(Content, letter.Content);
    }

    [Fact]
    public void Content_WhenNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => NewLetter(content: null!));
    }

    [Theory]
    [MemberData(nameof(BlankValues))]
    public void Content_WhenBlank_ThrowsArgumentException(string content)
    {
        Assert.Throws<ArgumentException>(() => NewLetter(content: content));
    }

    private static CoverLetter NewLetter(string title = Title, string content = Content) => new()
    {
        JobApplicationId = Guid.CreateVersion7(),
        Title = title,
        Content = content,
    };

    public static TheoryData<string> BlankValues =>
    [
        "",
        " ",
        "\t\n",
    ];
}
