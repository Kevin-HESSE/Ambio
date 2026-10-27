using System.Globalization;

namespace Ambio.Web.Components.Shared;

/// <summary>Sizes of files as the UI shows them.</summary>
public static class DisplaySizes
{
    private const long Kilobyte = 1024;
    private const long Megabyte = 1024 * Kilobyte;

    /// <summary>"184 KB" below a megabyte, rounded up so a small file never shows 0, then "1.2 MB" or "5 MB".</summary>
    public static string ToDisplaySize(this long bytes) =>
        bytes < Megabyte
            ? $"{Math.Max(1, (bytes + Kilobyte - 1) / Kilobyte)} KB"
            : $"{(bytes / (double)Megabyte).ToString("0.#", CultureInfo.InvariantCulture)} MB";
}
