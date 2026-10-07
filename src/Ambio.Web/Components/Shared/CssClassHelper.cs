namespace Ambio.Web.Components.Shared;

public static class CssClassHelper
{
    public static string Serialize(IEnumerable<string> defaultClass, Dictionary<string, bool> additionalClass)
    {
        var classList = new List<string>(defaultClass);

        foreach (var (key, value) in additionalClass)
        {
            if (value)
            {
                classList.Add(key);
            }
        }

        return string.Join(" ", classList);
    }
}
