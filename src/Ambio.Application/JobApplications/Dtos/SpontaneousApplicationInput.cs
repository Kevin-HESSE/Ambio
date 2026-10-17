using System.ComponentModel.DataAnnotations;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The fields of the application form, to create or edit an application sent to a company without any
/// offer.</summary>
public record SpontaneousApplicationInput : JobApplicationInput
{
    [Required(ErrorMessage = "Enter the position you apply for.")]
    [StringLength(200, ErrorMessage = "The position can't be longer than {1} characters.")]
    public string TargetPosition { get; set; } = "";
}
