using Ambio.Domain.Companies;

namespace Ambio.Web.Components.Companies.Shared;

/// <summary>Display names of the company enums. The database stores the enum names, never these labels.</summary>
public static class CompanyLabels
{
    public static string ToLabel(this CompanyKind kind) => kind switch
    {
        CompanyKind.Employer => "Employer",
        CompanyKind.RecruitmentAgency => "Recruitment agency",
        CompanyKind.ServiceCompany => "IT services",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    public static string ToLabel(this CompanySize size) => size switch
    {
        CompanySize.Micro => "1–10 employees",
        CompanySize.Small => "11–50 employees",
        CompanySize.Medium => "51–250 employees",
        CompanySize.Large => "251–1,000 employees",
        CompanySize.VeryLarge => "More than 1,000 employees",
        _ => throw new ArgumentOutOfRangeException(nameof(size), size, null),
    };
}
