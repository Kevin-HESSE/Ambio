using Ambio.Application.Common.Attributes;

namespace Ambio.Application.Tests.Common.Attributes;

public class EmailExpressionAttributeTests
{
    private readonly EmailExpressionAttribute _attribute = new();

    [Theory]
    [MemberData(nameof(EmailTestData.ValidEmails), MemberType = typeof(EmailTestData))]
    public void ValidEmail_IsValid(string email)
    {
        Assert.True(_attribute.IsValid(email));
    }

    [Theory]
    [MemberData(nameof(EmailTestData.InvalidEmails), MemberType = typeof(EmailTestData))]
    public void InvalidEmail_IsNotValid(string email)
    {
        Assert.False(_attribute.IsValid(email));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void NullOrEmpty_IsValid(string? email)
    {
        Assert.True(_attribute.IsValid(email));
    }

    [Fact]
    public void DefaultErrorMessage_ContainsFieldName()
    {
        Assert.Equal("The Email field is not a valid e-mail address.", _attribute.FormatErrorMessage("Email"));
    }
}
