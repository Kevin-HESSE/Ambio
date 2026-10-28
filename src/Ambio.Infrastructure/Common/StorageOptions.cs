namespace Ambio.Infrastructure.Common;

/// <summary>Where the documents are stored, and how large an uploaded one can be, from the <see cref="Section"/>
/// section.</summary>
public sealed class StorageOptions
{
    public const string Section = "Ambio:Storage";

    /// <summary>The folder of the ASP.NET Core DataProtection keys, under the root.</summary>
    public const string KeysFolder = "keys";

    /// <summary>The folder of the documents. A relative path starts from the app's content root: <c>Data</c> is
    /// <c>src/Ambio.Web/Data</c> in development, next to the database. The Docker image uses <c>/data</c>.</summary>
    public string Root { get; set; } = "Data";

    public int MaxUploadSizeInMegabytes { get; set; } = 5;

    /// <summary><see cref="MaxUploadSizeInMegabytes"/> in bytes.</summary>
    public long MaxUploadSize => MaxUploadSizeInMegabytes * 1024L * 1024L;

    /// <summary>The absolute path of <see cref="Root"/>, a relative root starting from
    /// <paramref name="contentRootPath"/>.</summary>
    public string GetFullRoot(string contentRootPath) =>
        Path.TrimEndingDirectorySeparator(Path.GetFullPath(Root, contentRootPath));

    /// <summary>A root, and a positive size limit.</summary>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Root) && MaxUploadSizeInMegabytes > 0;
}
