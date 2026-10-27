namespace Ambio.Web.Components.Shared;

/// <summary>One tab of a <see cref="TabNav"/>.</summary>
/// <param name="Value">Value of the tab: its <c>?tab=</c> value, or its route when each tab is a page.</param>
/// <param name="Label">Visible name.</param>
/// <param name="Href">Base-relative URL of the page showing this tab.</param>
/// <param name="Count">Number of items in the tab, shown next to its name when set.</param>
public sealed record TabNavItem(string Value, string Label, string Href, int? Count = null);
