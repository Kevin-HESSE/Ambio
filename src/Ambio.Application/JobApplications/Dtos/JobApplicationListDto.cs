using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>A page of the applications list: the first <paramref name="Applications"/> matching a filter, out of
/// <paramref name="Total"/>. <paramref name="CountsByStatus"/> holds every status, with the number of applications
/// that match the other criteria of the filter, whatever its status.</summary>
public record JobApplicationListDto(
    IReadOnlyList<JobApplicationSummaryDto> Applications,
    int Total,
    IReadOnlyDictionary<ApplicationStatus, int> CountsByStatus);
