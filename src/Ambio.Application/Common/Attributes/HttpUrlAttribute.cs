using System.ComponentModel.DataAnnotations;

namespace Ambio.Application.Common.Attributes;

/// <summary>An absolute http or https link to a domain name, such as https://acme.com. An empty value is valid:
/// add <see cref="RequiredAttribute"/> when the field is required.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HttpUrlAttribute : ValidationAttribute
{
    public HttpUrlAttribute() : base("Enter the full link, such as https://example.com")
    {
    }

    public override bool IsValid(object? value)
    {
        return value is null || value is "" || value is string url
            && Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
            && HasDomainName(uri);
    }

    // "https://acme" is an absolute URI, but not a site: the host needs a dot and an extension made of letters.
    private static bool HasDomainName(Uri uri)
    {
        var labels = uri.Host.Split('.');
        return uri.HostNameType == UriHostNameType.Dns
            && labels.Length >= 2
            && labels.All(label => label.Length > 0)
            && labels[^1].Length >= 2
            && labels[^1].All(char.IsLetter);
    }
}
