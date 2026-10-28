using System.ComponentModel.DataAnnotations;

using Ambio.Application.Tests.Common.Attributes;
using Ambio.Application.Users.Dtos;

namespace Ambio.Application.Tests.Users.Dtos;

public class RegisterInputValidationTests
{
    [Fact]
    public void ValidInput_HasNoErrors()
    {
        Assert.Empty(Validate(ValidInput()));
    }

    [Theory]
    [MemberData(nameof(EmailTestData.ValidEmails), MemberType = typeof(EmailTestData))]
    public void ValidEmail_HasNoErrors(string email)
    {
        Assert.Empty(Validate(ValidInput() with { Email = email }));
    }

    [Theory]
    [MemberData(nameof(EmailTestData.InvalidEmails), MemberType = typeof(EmailTestData))]
    public void InvalidEmail_FailsOnEmail(string email)
    {
        var results = Validate(ValidInput() with { Email = email });

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterInput.Email)));
    }

    [Fact]
    public void EmptyEmail_FailsOnEmail()
    {
        var results = Validate(ValidInput() with { Email = "" });

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterInput.Email)));
    }

    [Fact]
    public void PasswordShorterThanSixCharacters_FailsOnPassword()
    {
        var results = Validate(ValidInput() with { Password = "Ab1!" });

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterInput.Password)));
    }

    private static RegisterInput ValidInput() => new()
    {
        Email = "user@example.com",
        Password = "Passw0rd!",
    };

    private static List<ValidationResult> Validate(RegisterInput input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }
}
