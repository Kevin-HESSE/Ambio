using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

namespace Ambio.Application.Dashboard.Dtos;

/// <summary>An event of an <paramref name="Application"/>, at <paramref name="OccurredAt"/> (UTC):
/// <paramref name="Status"/> is the status it reached, <see cref="ApplicationStatus.Draft"/> for its creation, as no
/// change leads back to a draft.</summary>
public record ActivityDto(
    JobApplicationSummaryDto Application,
    ApplicationStatus Status,
    DateTime OccurredAt);
