using Ambio.Application.CvDocuments;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Ambio.Web.Components.CvDocuments;

internal static class CvDocumentEndpoints
{
    private const string PdfContentType = "application/pdf";

    /// <summary>Serves the file of a CV (<see cref="CvDocumentRoutes"/>) to the logged-in user: inline for the preview,
    /// as an attachment for the download, under the name it was uploaded with. An unknown CV, or one whose file is
    /// missing, is a 404.</summary>
    public static IEndpointConventionBuilder MapCvDocumentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var group = endpoints.MapGroup($"/{CvDocumentRoutes.Base}").RequireAuthorization();

        group.MapGet("/{id:guid}", (Guid id, [FromServices] ICvDocumentService service, HttpContext context) =>
            SendAsync(id, download: false, service, context));

        group.MapGet("/{id:guid}/download", (Guid id, [FromServices] ICvDocumentService service, HttpContext context) =>
            SendAsync(id, download: true, service, context));

        return group;
    }

    private static async Task<IResult> SendAsync(Guid id, bool download, ICvDocumentService service, HttpContext context)
    {
        var result = await service.OpenAsync(id, context.RequestAborted);
        if (!result.TryGetValue(out var file))
        {
            return TypedResults.NotFound();
        }

        // The content was checked as a PDF on upload: the browser must not guess another type.
        context.Response.Headers.XContentTypeOptions = "nosniff";

        if (download)
        {
            return TypedResults.File(file.Content, PdfContentType, file.FileName, enableRangeProcessing: true);
        }

        // The file name still names the PDF when it is saved from the browser's viewer.
        var disposition = new ContentDispositionHeaderValue("inline");
        disposition.SetHttpFileName(file.FileName);
        context.Response.Headers.ContentDisposition = disposition.ToString();

        return TypedResults.File(file.Content, PdfContentType, enableRangeProcessing: true);
    }
}
