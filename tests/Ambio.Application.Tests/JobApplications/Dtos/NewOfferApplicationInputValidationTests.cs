using System.ComponentModel.DataAnnotations;

using Ambio.Application.JobApplications.Dtos;

namespace Ambio.Application.Tests.JobApplications.Dtos;

public class NewOfferApplicationInputValidationTests
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

        Assert.Equal([nameof(NewOfferApplicationInput.SentOn)], result.MemberNames);
    }

    [Fact]
    public void WithoutOffer_FailsOnJobOfferId()
    {
        var result = Assert.Single(Validate(ValidInput() with { JobOfferId = null }));

        Assert.Equal([nameof(NewOfferApplicationInput.JobOfferId)], result.MemberNames);
    }

    private static NewOfferApplicationInput ValidInput() => new() { JobOfferId = Guid.CreateVersion7() };

    private static List<ValidationResult> Validate(NewOfferApplicationInput input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }
}
