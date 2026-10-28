namespace Ambio.Web.Components.Shared;

public static class CssClassHelper
{
    public static string Serialize(
        IEnumerable<string> defaultClass,
        Dictionary<string, bool> additionalClass,
        IReadOnlyDictionary<string, object>? additionalAttributes = null)
    {
        var classList = new List<string>(defaultClass);

        classList.AddRange(GetClassValue(additionalAttributes));

        foreach (var (key, value) in additionalClass)
        {
            if (value)
            {
                classList.Add(key);
            }
        }

        return string.Join(" ", classList);
    }

    private static List<string> GetClassValue(IReadOnlyDictionary<string, object>? additionalAttributes)
    {
        return additionalAttributes?.GetValueOrDefault("class") is string value && value.Length > 0
            ? [value]
            : [];
    }
}
