namespace Ambio.Application.JobApplications.Dtos;

/// <summary>What the URL of an application is built from (ADR 0014), such as after creating it.</summary>
public record JobApplicationLinkDto(
    string ShortId,
    string Slug);
