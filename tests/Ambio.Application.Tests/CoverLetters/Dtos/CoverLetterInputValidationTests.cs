using System.ComponentModel.DataAnnotations;

using Ambio.Application.CoverLetters.Dtos;

namespace Ambio.Application.Tests.CoverLetters.Dtos;

public class CoverLetterInputValidationTests
{
    private const string Title = "Cover letter · Brightwave";
    private const string Content = "Dear Brightwave team,";

    [Fact]
    public void Empty_FailsOnTitleAndContent()
    {
        var results = Validate(new CoverLetterInput());

        Assert.Equal(
            [[nameof(CoverLetterInput.Title)], [nameof(CoverLetterInput.Content)]],
            results.Select(r => r.MemberNames.ToArray()));
    }

    [Fact]
    public void BlankContent_FailsOnContent()
    {
        var result = Assert.Single(Validate(new CoverLetterInput { Title = Title, Content = " \n" }));

        Assert.Equal([nameof(CoverLetterInput.Content)], result.MemberNames);
    }

    [Fact]
    public void WithTitleAndContent_HasNoErrors()
    {
        Assert.Empty(Validate(new CoverLetterInput { Title = Title, Content = Content }));
    }

    private static List<ValidationResult> Validate(CoverLetterInput input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }
}
