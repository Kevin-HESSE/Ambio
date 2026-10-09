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
}
