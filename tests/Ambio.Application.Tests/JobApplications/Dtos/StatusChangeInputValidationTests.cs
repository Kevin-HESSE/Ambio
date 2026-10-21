using System.ComponentModel.DataAnnotations;

using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

namespace Ambio.Application.Tests.JobApplications.Dtos;

public class StatusChangeInputValidationTests
{
    private static readonly DateOnly SentOn = new(2026, 9, 3);

    [Fact]
    public void WithoutStatus_FailsOnTo()
    {
        var result = Assert.Single(Validate(new StatusChangeInput()));

        Assert.Equal([nameof(StatusChangeInput.To)], result.MemberNames);
    }

    [Fact]
    public void AppliedWithoutDate_FailsOnSentOn()
    {
        var result = Assert.Single(Validate(new StatusChangeInput { To = ApplicationStatus.Applied }));

        Assert.Equal(JobApplicationInput.SentOnRequired, result.ErrorMessage);
        Assert.Equal([nameof(StatusChangeInput.SentOn)], result.MemberNames);
    }

    [Fact]
    public void AppliedWithDate_HasNoErrors()
    {
        Assert.Empty(Validate(new StatusChangeInput { To = ApplicationStatus.Applied, SentOn = SentOn }));
    }

    [Fact]
    public void OtherStatusWithoutDate_HasNoErrors()
    {
        Assert.Empty(Validate(new StatusChangeInput { To = ApplicationStatus.Withdrawn }));
    }

    private static List<ValidationResult> Validate(StatusChangeInput input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }
}
