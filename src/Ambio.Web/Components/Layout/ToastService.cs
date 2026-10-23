namespace Ambio.Web.Components.Layout;

/// <summary>The toast of the current request, shown by <see cref="ToastHost"/> in the layout. Scoped: a page shows it
/// from the handler of its static form, and the response carries it; the next request starts without one.</summary>
public class ToastService
{
    public Toast? Current { get; private set; }

    /// <summary>Raised when <see cref="Current"/> changes, so the host renders it.</summary>
    public event Action? Changed;

    public void Show(Toast toast)
    {
        Current = toast;
        Changed?.Invoke();
    }
}
