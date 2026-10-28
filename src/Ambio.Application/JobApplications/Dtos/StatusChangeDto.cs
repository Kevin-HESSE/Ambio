using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>A change in the status history of an application; <paramref name="ChangedAt"/> is UTC.</summary>
public record StatusChangeDto(
    ApplicationStatus From,
    ApplicationStatus To,
    DateTime ChangedAt,
    string? Comment);
