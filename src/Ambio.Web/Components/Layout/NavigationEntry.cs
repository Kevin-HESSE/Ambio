using Microsoft.AspNetCore.Components.Routing;

namespace Ambio.Web.Components.Layout;

/// <summary>A section of the app, shown in the sidebar on desktop and in the tab bar on mobile.</summary>
/// <param name="Label">Name in the sidebar.</param>
/// <param name="TabLabel">Shorter name in the tab bar.</param>
/// <param name="Href">Base-relative URL of the section.</param>
/// <param name="Icon">Bootstrap Icons name, without the <c>bi-</c> prefix.</param>
/// <param name="ActiveIcon">Icon shown while the section is active (usually the filled variant).</param>
/// <param name="Match">How the current URL is matched against <paramref name="Href"/>.</param>
public sealed record NavigationEntry(
    string Label,
    string TabLabel,
    string Href,
    string Icon,
    string ActiveIcon,
    NavLinkMatch Match = NavLinkMatch.Prefix);
