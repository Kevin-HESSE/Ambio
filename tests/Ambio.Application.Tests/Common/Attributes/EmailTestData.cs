namespace Ambio.Application.Tests.Common.Attributes;

public static class EmailTestData
{
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
        "user@example.com\n",
    ];
}
