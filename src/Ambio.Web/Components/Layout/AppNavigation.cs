using Ambio.Web.Components.Companies;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.Offers;
using Ambio.Web.Components.Settings;

using Microsoft.AspNetCore.Components.Routing;

namespace Ambio.Web.Components.Layout;

/// <summary>
/// The sections of the main navigation (docs/ui.md#navigation). Only pages that exist are listed:
/// each feature adds its entry with its first page.
/// </summary>
public static class AppNavigation
{
    /// <summary>The "Tracking" group of the sidebar, also the tabs of the mobile tab bar (before "More").</summary>
    public static IReadOnlyList<NavigationEntry> Tracking { get; } =
    [
        new("Dashboard", "Home", "", "house-door", "house-door-fill", NavLinkMatch.All),
        new("Applications", "Applications", JobApplicationRoutes.List, "card-checklist", "card-checklist"),
        new("Offers", "Offers", OfferRoutes.List, "briefcase", "briefcase-fill"),
        new("Companies", "Companies", CompanyRoutes.List, "building", "building-fill"),
    ];

    /// <summary>Base-relative paths of the pages reached from the More sheet, where the "More" tab is active.</summary>
    public static IReadOnlyList<string> MoreRoutes { get; } =
    [
        UserSettingsRoutes.Edit,
        "Account/Manage",
    ];
}
