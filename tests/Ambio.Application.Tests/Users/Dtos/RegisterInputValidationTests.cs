using System.ComponentModel.DataAnnotations;

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
    [MemberData(nameof(ValidEmails))]
    public void ValidEmail_HasNoErrors(string email)
    {
        Assert.Empty(Validate(ValidInput() with { Email = email }));
    }

    [Theory]
    [MemberData(nameof(InvalidEmails))]
    public void InvalidEmail_FailsOnEmail(string email)
    {
        var results = Validate(ValidInput() with { Email = email });

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterInput.Email)));
    }

    [Fact]
    public void PasswordShorterThanSixCharacters_FailsOnPassword()
    {
        var results = Validate(ValidInput() with { Password = "Ab1!", ConfirmPassword = "Ab1!" });

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterInput.Password)));
    }

    [Fact]
    public void MismatchedConfirmation_FailsOnConfirmPassword()
    {
        var results = Validate(ValidInput() with { ConfirmPassword = "Other0rd!" });

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RegisterInput.ConfirmPassword)));
    }

    private static RegisterInput ValidInput() => new()
    {
        Email = "user@example.com",
        Password = "Passw0rd!",
        ConfirmPassword = "Passw0rd!",
    };

    private static List<ValidationResult> Validate(RegisterInput input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }

    public static TheoryData<string> ValidEmails =>
    [
        "first.last@example.com",
        "first+tag@example.com",
        "user@sub.example.co.uk",
        "user@my-domain.io",
        "USER_42@EXAMPLE.COM",
    ];

    public static TheoryData<string> InvalidEmails =>
    [
        "",
        "not-an-email",
        "user.example.com",
        "user@",
        "@example.com",
        "user@@example.com",
        "user@exa@mple.com",
        "user@example",
        "user@example.",
        "user@localhost",
        "user @example.com",
        "user@exam ple.com",
        "user@.com",
        "user@example..com",
        ".user@example.com",
        "user.@example.com",
        "us..er@example.com",
        "user@-example.com",
        "user@example-.com",
        "us(er@example.com",
        "user@example.c",
    ];
}
