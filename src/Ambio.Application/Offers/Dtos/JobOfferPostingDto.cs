namespace Ambio.Application.Offers.Dtos;

/// <summary>A site where an offer was seen. <paramref name="SeenAt"/> (UTC) is the day it was recorded.</summary>
public record JobOfferPostingDto(
    Guid Id,
    string Site,
    string Url,
    DateTime SeenAt);
