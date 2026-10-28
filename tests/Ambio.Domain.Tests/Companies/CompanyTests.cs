using Ambio.Domain.Companies;

namespace Ambio.Domain.Tests.Companies;

public class CompanyTests
{
    private const string CompanyName = "Acme";
    private const string CompanySlug = "acme";

    private static readonly DateTime ArchivedAt = new(2026, 9, 27, 10, 0, 0, DateTimeKind.Utc);

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

    [Fact]
    public void Archive_Active_RecordsTheDay()
    {
        var company = NewCompany();

        company.Archive(ArchivedAt);

        Assert.Equal(ArchivedAt, company.ArchivedAt);
        Assert.True(company.IsArchived);
    }

    [Fact]
    public void Archive_AlreadyArchived_KeepsTheFirstDay()
    {
        var company = NewCompany();
        company.Archive(ArchivedAt);

        company.Archive(ArchivedAt.AddDays(1));

        Assert.Equal(ArchivedAt, company.ArchivedAt);
    }

    [Fact]
    public void Restore_Archived_ClearsTheDay()
    {
        var company = NewCompany();
        company.Archive(ArchivedAt);

        company.Restore();

        Assert.Null(company.ArchivedAt);
        Assert.False(company.IsArchived);
    }

    private static Company NewCompany(string name = CompanyName) => new()
    {
        Name = name,
        Slug = CompanySlug,
        Kind = CompanyKind.Employer,
    };

    public static TheoryData<string> BlankNames =>
    [
        "",
        " ",
        "\t\n",
    ];
}
