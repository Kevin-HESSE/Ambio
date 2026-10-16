using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>A row of the applications lists: its title is the offer's, or the target position of a spontaneous
/// application, and its company the one it was sent to. <paramref name="AppliedVia"/> is the site of the posting used
/// to apply, when known; <paramref name="SentOn"/> is null while the application is a draft.</summary>
public record JobApplicationSummaryDto(
    Guid Id,
    ApplicationKind Kind,
    string Title,
    string CompanyName,
    ApplicationStatus Status,
    Channel Channel,
    string? AppliedVia,
    DateOnly? SentOn);
