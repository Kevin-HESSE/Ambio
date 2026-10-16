using System.ComponentModel.DataAnnotations;

using Ambio.Application.JobApplications.Dtos;

namespace Ambio.Application.Tests.JobApplications.Dtos;

public class NewSpontaneousApplicationInputValidationTests
{
    private static readonly DateOnly SentOn = new(2026, 9, 3);

    [Fact]
    public void Draft_HasNoErrors()
    {
        Assert.Empty(Validate(ValidInput()));
    }

    [Fact]
    public void AlreadySentWithDate_HasNoErrors()
    {
        Assert.Empty(Validate(ValidInput() with { AlreadySent = true, SentOn = SentOn }));
    }

    [Fact]
    public void AlreadySentWithoutDate_FailsOnSentOn()
    {
        var result = Assert.Single(Validate(ValidInput() with { AlreadySent = true }));

        Assert.Equal([nameof(NewSpontaneousApplicationInput.SentOn)], result.MemberNames);
    }

    [Fact]
    public void WithoutCompany_FailsOnCompanyName()
    {
        var result = Assert.Single(Validate(ValidInput() with { CompanyName = "" }));

        Assert.Equal([nameof(NewSpontaneousApplicationInput.CompanyName)], result.MemberNames);
    }

    [Fact]
    public void WithoutPosition_FailsOnTargetPosition()
    {
        var result = Assert.Single(Validate(ValidInput() with { TargetPosition = " " }));

        Assert.Equal([nameof(NewSpontaneousApplicationInput.TargetPosition)], result.MemberNames);
    }

    [Theory]
    [InlineData(nameof(NewSpontaneousApplicationInput.CompanyName))]
    [InlineData(nameof(NewSpontaneousApplicationInput.TargetPosition))]
    public void TooLongText_FailsOnItsField(string field)
    {
        var tooLong = new string('a', 201);
        var input = field == nameof(NewSpontaneousApplicationInput.CompanyName)
            ? ValidInput() with { CompanyName = tooLong }
            : ValidInput() with { TargetPosition = tooLong };

        var result = Assert.Single(Validate(input));

        Assert.Equal([field], result.MemberNames);
    }

    private static NewSpontaneousApplicationInput ValidInput() =>
        new() { CompanyName = "Lumen Health", TargetPosition = "Backend Developer" };

    private static List<ValidationResult> Validate(NewSpontaneousApplicationInput input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }
}
