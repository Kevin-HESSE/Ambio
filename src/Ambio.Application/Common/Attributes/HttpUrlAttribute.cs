using System.ComponentModel.DataAnnotations;

namespace Ambio.Application.Common.Attributes;

/// <summary>An absolute http or https link. An empty value is valid: add <see cref="RequiredAttribute"/> when the field is required.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HttpUrlAttribute : ValidationAttribute
{
    public HttpUrlAttribute() : base("Enter the full link, starting with https://")
    {
    }

    public override bool IsValid(object? value)
    {
        return value is null || value is "" || value is string url
            && Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }
}
