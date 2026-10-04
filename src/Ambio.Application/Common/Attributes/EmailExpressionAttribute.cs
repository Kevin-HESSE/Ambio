using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Ambio.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed partial class EmailExpressionAttribute : ValidationAttribute
{
    private const string Email =
        @"^[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*@([A-Za-z0-9]([A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z]{2,}\z";

    public EmailExpressionAttribute() : base("The {0} field is not a valid e-mail address.")
    {
    }

    public override bool IsValid(object? value)
    {
        return value is null || value is "" || value is string email && EmailRegex().IsMatch(email);
    }

    [GeneratedRegex(Email)]
    private static partial Regex EmailRegex();
}
