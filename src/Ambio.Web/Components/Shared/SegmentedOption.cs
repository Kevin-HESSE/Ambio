namespace Ambio.Web.Components.Shared;

/// <summary>One choice of a <see cref="SegmentedControl"/>.</summary>
/// <param name="Value">Value of the radio button.</param>
/// <param name="Label">Visible label, or accessible name only when the control shows icons only.</param>
/// <param name="Icon">Bootstrap Icons name, without the <c>bi-</c> prefix.</param>
public sealed record SegmentedOption(string Value, string Label, string? Icon = null);
