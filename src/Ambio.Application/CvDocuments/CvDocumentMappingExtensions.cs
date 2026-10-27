using Ambio.Application.CvDocuments.Dtos;
using Ambio.Domain.CvDocuments;

namespace Ambio.Application.CvDocuments;

public static class CvDocumentMappingExtensions
{
    /// <summary>Projection translated to SQL by EF Core.</summary>
    public static IQueryable<CvDocumentDto> ToDto(this IQueryable<CvDocument> documents) =>
        documents.Select(d => new CvDocumentDto(
            Id: d.Id,
            FileName: d.FileName,
            Size: d.Size,
            UploadedAt: d.CreatedAt));
}
