namespace Ambio.Application.Offers.Dtos;

/// <summary>What the URL of an offer is built from (ADR 0025), such as after creating it.</summary>
public record JobOfferLinkDto(
    string ShortId,
    string Slug);
