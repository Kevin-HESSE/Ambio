using Ambio.Domain.JobApplications;
using Ambio.Web.Components.Shared;

namespace Ambio.Web.Tests.Components.Shared;

public class QueryValueTests
{
    [Fact]
    public void Of_EnumValue_ReturnsItsNameInLowercase()
    {
        Assert.Equal("interview", QueryValue.Of(ApplicationStatus.Interview));
    }

    [Theory]
    [InlineData("interview")]
    [InlineData("INTERVIEW")]
    public void Parse_NameInAnyCase_ReturnsTheValue(string value)
    {
        Assert.Equal(ApplicationStatus.Interview, QueryValue.Parse<ApplicationStatus>(value));
    }

    [Theory]
    [MemberData(nameof(InvalidValues))]
    public void Parse_UnknownTextOrNumber_ReturnsNull(string value)
    {
        Assert.Null(QueryValue.Parse<ApplicationStatus>(value));
    }

    [Fact]
    public void Parse_Null_ReturnsNull()
    {
        Assert.Null(QueryValue.Parse<ApplicationStatus>(null));
    }

    public static TheoryData<string> InvalidValues => new()
    {
        "",
        "hired",
        "2",
    };
}
