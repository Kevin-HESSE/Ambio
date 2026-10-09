namespace Ambio.Web.Components.Shared;

/// <summary>
/// Color variant of a <see cref="ButtonLink"/>, styled in <c>wwwroot/css/buttons.css</c>.
/// </summary>
public enum ButtonVariant
{
    /// <summary>Plain <c>btn</c>: no background, secondary text for an icon-only button.</summary>
    Default,

    /// <summary>The page's main action (<c>btn-primary</c>).</summary>
    Primary,

    /// <summary>A destructive action (<c>btn-danger</c>).</summary>
    Danger,
}
