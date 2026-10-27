using Ambio.Application.Common;
using Ambio.Application.CvDocuments.Dtos;

namespace Ambio.Application.CvDocuments;

public interface ICvDocumentService
{
    /// <summary>The largest PDF accepted by <see cref="UploadAsync"/>, in bytes.</summary>
    public long MaxUploadSize { get; }

    /// <summary>The CVs sent with the application, archived or not, newest first. Empty when the application has
    /// none or doesn't exist.</summary>
    public Task<IReadOnlyList<CvDocumentDto>> ListAsync(Guid applicationId, CancellationToken cancellationToken = default);

    /// <summary>Stores the PDF sent with the application, even an archived one, and returns the id of its CV. The
    /// content is checked, not the name. Fails with <see cref="CvDocumentErrors.ApplicationNotFound"/>,
    /// <see cref="CvDocumentErrors.TooLarge"/> (over <see cref="MaxUploadSize"/>), <see cref="CvDocumentErrors.NotPdf"/>
    /// or <see cref="CvDocumentErrors.UploadFailed"/>.</summary>
    public Task<ServiceResult<Guid>> UploadAsync(Guid applicationId, string fileName, Stream content, CancellationToken cancellationToken = default);

    /// <summary>The file of the CV, to preview or download. Fails with <see cref="CvDocumentErrors.NotFound"/>, or
    /// <see cref="CvDocumentErrors.FileMissing"/> when its file was removed from the storage (ADR 0005).</summary>
    public Task<ServiceResult<CvFileDto>> OpenAsync(Guid id, CancellationToken cancellationToken = default);
}
