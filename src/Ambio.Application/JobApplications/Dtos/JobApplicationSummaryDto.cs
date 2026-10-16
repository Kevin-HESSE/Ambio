using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>A row of the applications lists. <paramref name="AppliedVia"/> is the site of the posting used to apply,
/// when known; <paramref name="SentOn"/> is null while the application is a draft.</summary>
public record JobApplicationSummaryDto(
    Guid Id,
    string Title,
    string CompanyName,
    ApplicationStatus Status,
    Channel Channel,
    string? AppliedVia,
    DateOnly? SentOn);
