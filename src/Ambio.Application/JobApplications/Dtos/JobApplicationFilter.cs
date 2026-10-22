using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The criteria of the applications list: each one left null keeps every application. The company is the one
/// with <paramref name="CompanySlug"/>, of the offer or of a spontaneous application; <paramref name="Search"/> is
/// matched against the title and the company, ignoring case and accents.</summary>
public record JobApplicationFilter(
    ApplicationStatus? Status = null,
    string? CompanySlug = null,
    ApplicationKind? Kind = null,
    string? Search = null)
{
    /// <summary>Whether no criterion is set: a blank search counts as none.</summary>
    public bool IsEmpty => Status is null && CompanySlug is null && Kind is null && string.IsNullOrWhiteSpace(Search);
}
