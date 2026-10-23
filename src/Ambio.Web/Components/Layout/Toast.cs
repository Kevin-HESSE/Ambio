namespace Ambio.Web.Components.Layout;

/// <summary>A short message shown by <see cref="ToastHost"/> after an action, such as "Application archived", with an
/// optional <paramref name="Icon"/> (Bootstrap Icons name, without the <c>bi-</c> prefix) and an action.</summary>
public record Toast(string Message, string? Icon = null, ToastAction? Action = null);

/// <summary>The button of a <see cref="Toast"/>, such as Undo: it sends the form of the page with this
/// <paramref name="FormId"/>.</summary>
public record ToastAction(string Label, string FormId);
