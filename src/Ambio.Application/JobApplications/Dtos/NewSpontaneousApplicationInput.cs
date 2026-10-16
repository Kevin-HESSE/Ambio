using System.ComponentModel.DataAnnotations;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The fields of the new application form, for an application sent to a company without any offer.</summary>
public record NewSpontaneousApplicationInput : NewJobApplicationInput
{
    /// <summary>The name of an existing company, compared ignoring case, or of a company to create.</summary>
    [Required(ErrorMessage = "Choose the company you apply to.")]
    [StringLength(200, ErrorMessage = "The company name can't be longer than {1} characters.")]
    public string CompanyName { get; set; } = "";

    [Required(ErrorMessage = "Enter the position you apply for.")]
    [StringLength(200, ErrorMessage = "The position can't be longer than {1} characters.")]
    public string TargetPosition { get; set; } = "";
}
