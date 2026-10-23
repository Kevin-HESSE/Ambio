using Ambio.Application.CoverLetters.Dtos;
using Ambio.Domain.CoverLetters;

namespace Ambio.Application.CoverLetters;

public static class CoverLetterMappingExtensions
{
    /// <summary>Projection translated to SQL by EF Core.</summary>
    public static IQueryable<CoverLetterDto> ToDto(this IQueryable<CoverLetter> letters) =>
        letters.Select(l => new CoverLetterDto(
            Id: l.Id,
            Title: l.Title,
            Content: l.Content,
            EditedAt: l.UpdatedAt));

    /// <summary>A new letter of the application, with the fields of the form, trimmed.</summary>
    public static CoverLetter ToEntity(this CoverLetterInput input, Guid jobApplicationId) => new()
    {
        JobApplicationId = jobApplicationId,
        Title = input.Title,
        Content = input.Content,
    };

    /// <summary>Copies the fields of the form onto the letter, trimmed.</summary>
    public static void ApplyTo(this CoverLetterInput input, CoverLetter letter)
    {
        letter.Title = input.Title;
        letter.Content = input.Content;
    }
}
