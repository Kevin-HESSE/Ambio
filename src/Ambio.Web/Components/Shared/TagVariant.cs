namespace Ambio.Web.Components.Shared;

/// <summary>Colors of a <see cref="Tag"/> (docs/design-system.md#tags).</summary>
public enum TagVariant
{
    /// <summary>Every attribute that isn't a status: the icon tells them apart (<c>tag/neutral</c>).</summary>
    Neutral,

    /// <summary>Spontaneous applications only, as there is no offer behind them (<c>tag/accent</c>).</summary>
    Accent,

    /// <summary>Archived rows and items only (<c>tag/archived</c>), see <see cref="ArchivedTag"/>.</summary>
    Archived,
}
