using Ambio.Domain.Companies;

namespace Ambio.Domain.Tests.Companies;

public class CompanyTests
{
    private const string CompanyName = "Acme";

    [Fact]
    public void New_GeneratesVersion7Id()
    {
        Assert.Equal(7, NewCompany().Id.Version);
    }

    [Fact]
    public void New_GeneratesDistinctIds()
    {
        Assert.NotEqual(NewCompany().Id, NewCompany().Id);
    }

    [Fact]
    public void Name_WithSurroundingWhitespace_IsTrimmed()
    {
        var company = NewCompany($"  {CompanyName}\t");

        Assert.Equal(CompanyName, company.Name);
    }

    [Fact]
    public void Name_WhenNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => NewCompany(null!));
    }

    [Theory]
    [MemberData(nameof(BlankNames))]
    public void Name_WhenBlank_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => NewCompany(name));
    }

    [Theory]
    [MemberData(nameof(BlankNames))]
    public void Name_RenamedToBlank_KeepsPreviousName(string name)
    {
        var company = NewCompany();

        Assert.Throws<ArgumentException>(() => company.Name = name);
        Assert.Equal(CompanyName, company.Name);
    }

    private static Company NewCompany(string name = CompanyName) => new()
    {
        Name = name,
        Kind = CompanyKind.Employer,
    };

    public static TheoryData<string> BlankNames =>
    [
        "",
        " ",
        "\t\n",
    ];
}
