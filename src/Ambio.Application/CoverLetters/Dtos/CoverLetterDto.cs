namespace Ambio.Application.CoverLetters.Dtos;

/// <summary>A cover letter of an application, in plain text; <paramref name="EditedAt"/> is UTC.</summary>
public record CoverLetterDto(
    Guid Id,
    string Title,
    string Content,
    DateTime EditedAt);
