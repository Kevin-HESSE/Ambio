using System.ComponentModel.DataAnnotations;

namespace Ambio.Application.CoverLetters.Dtos;

/// <summary>The fields of the cover letter editor: its title and its text.</summary>
public record CoverLetterInput
{
    [Required(ErrorMessage = "Enter a title for the letter.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Enter the text of the letter.")]
    public string Content { get; set; } = "";
}
