namespace Ambio.Web.Components.CvDocuments;

/// <summary>Base-relative URLs of the CV files, served by <see cref="CvDocumentEndpoints"/> outside the Blazor pages.
/// Their links skip enhanced navigation (<c>data-enhance-nav="false"</c>), which would fetch the PDF as a page.</summary>
public static class CvDocumentRoutes
{
    public const string Base = "cvs";

    /// <summary>The PDF shown by the browser, in a new tab.</summary>
    public static string Preview(Guid id) => $"{Base}/{id}";

    /// <summary>The PDF saved under the name it was uploaded with.</summary>
    public static string Download(Guid id) => $"{Base}/{id}/download";
}
