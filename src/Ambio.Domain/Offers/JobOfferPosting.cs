using Ambio.Domain.Common;

namespace Ambio.Domain.Offers;

/// <summary>A site where a job offer was seen. Created with <see cref="JobOffer.AddPosting"/>; it was seen on
/// the day it was recorded, its <see cref="CreatedAt"/>.</summary>
public class JobOfferPosting : ITimeStampable
{
    internal JobOfferPosting()
    {
    }

    public Guid Id { get; } = Guid.CreateVersion7();

    public required Guid JobOfferId { get; init; }

    public required string Site
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value.Trim();
        }
    }

    public required string Url
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value.Trim();
        }
    }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
