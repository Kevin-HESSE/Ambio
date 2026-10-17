using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>An application shown on its page, of either kind: <see cref="OfferApplicationDetailsDto"/> or
/// <see cref="SpontaneousApplicationDetailsDto"/>. Its title is the offer's, or the target position of a spontaneous
/// application; <paramref name="SentOn"/> is null while it is a draft, and <paramref name="CreatedAt"/> is UTC.</summary>
public abstract record JobApplicationDetailsDto(
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
    DateTime CreatedAt)
{
    /// <summary>The readable part of its URL, after the <see cref="ShortId"/>.</summary>
    public string Slug => JobApplicationSlug.From(Title, CompanyName);
}
