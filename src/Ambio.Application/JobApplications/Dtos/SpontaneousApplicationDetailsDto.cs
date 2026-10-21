using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>A spontaneous application: its title is the target position, and <paramref name="CompanyLocation"/> the
/// location of its company.</summary>
public record SpontaneousApplicationDetailsDto(
    Guid Id,
    string ShortId,
    string Title,
    string CompanyName,
    string CompanySlug,
    ApplicationStatus Status,
    Channel Channel,
    string? ChannelDetail,
    DateOnly? SentOn,
    string? Notes,
    DateTime CreatedAt,
    IReadOnlyList<StatusChangeDto> StatusHistory,
    string? CompanyLocation)
    : JobApplicationDetailsDto(Id, ShortId, Title, CompanyName, CompanySlug, Status, Channel, ChannelDetail, SentOn, Notes, CreatedAt, StatusHistory);
