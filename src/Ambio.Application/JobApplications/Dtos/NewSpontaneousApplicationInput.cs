using System.ComponentModel.DataAnnotations;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The fields of the new application form, for an application sent to a company without any offer: the
/// company comes first.</summary>
public record NewSpontaneousApplicationInput : SpontaneousApplicationInput
{
    /// <summary>The name of an existing company, compared ignoring case, or of a company to create.</summary>
    [Required(ErrorMessage = "Choose the company you apply to.")]
    [StringLength(200, ErrorMessage = "The company name can't be longer than {1} characters.")]
    public string CompanyName { get; set; } = "";
}
