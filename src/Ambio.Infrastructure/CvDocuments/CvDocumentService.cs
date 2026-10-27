using Ambio.Application.Common;
using Ambio.Application.CvDocuments;
using Ambio.Application.CvDocuments.Dtos;
using Ambio.Domain.CvDocuments;
using Ambio.Infrastructure.Common;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ambio.Infrastructure.CvDocuments;

internal class CvDocumentService : ICvDocumentService
{
    // Every PDF starts with this header, whatever its version.
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IDocumentStorage _storage;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CvDocumentService> _logger;

    public CvDocumentService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        IDocumentStorage storage,
        TimeProvider timeProvider,
        IOptions<StorageOptions> options,
        ILogger<CvDocumentService> logger)
    {
        _dbFactory = dbFactory;
        _storage = storage;
        _timeProvider = timeProvider;
        _logger = logger;
        MaxUploadSize = options.Value.MaxUploadSize;
    }

    public long MaxUploadSize { get; }

    public async Task<IReadOnlyList<CvDocumentDto>> ListAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // GUID v7 keys follow the creation order.
        return await db.CvDocuments
            .AsNoTracking()
            .Where(d => d.JobApplicationId == applicationId)
            .OrderByDescending(d => d.CreatedAt)
            .ThenByDescending(d => d.Id)
            .ToDto()
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<Guid>> UploadAsync(Guid applicationId, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        if (!await db.JobApplications.AnyAsync(a => a.Id == applicationId, cancellationToken))
        {
            return ServiceResult<Guid>.Failure(CvDocumentErrors.ApplicationNotFound);
        }

        // Read whole, within the limit, to check the content before anything is stored.
        using var file = new MemoryStream();
        if (!await TryCopyWithinLimitAsync(content, file, cancellationToken))
        {
            return ServiceResult<Guid>.Failure(CvDocumentErrors.TooLarge);
        }

        if (!IsPdf(file))
        {
            return ServiceResult<Guid>.Failure(CvDocumentErrors.NotPdf);
        }

        var document = CvDocument.Upload(applicationId, fileName, file.Length, Now);

        // The file is named after the id, so it is written first; it goes away if its row can't be saved.
        try
        {
            file.Position = 0;
            await _storage.SaveAsync(document.StoragePath, file, cancellationToken);
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "Failed to store the file of the CV {CvDocumentId}.", document.Id);
            return ServiceResult<Guid>.Failure(CvDocumentErrors.UploadFailed);
        }

        try
        {
            db.CvDocuments.Add(document);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Failed to save the CV {CvDocumentId}; its file is removed.", document.Id);
            await _storage.DeleteAsync(document.StoragePath, CancellationToken.None);
            return ServiceResult<Guid>.Failure(CvDocumentErrors.UploadFailed);
        }

        return ServiceResult<Guid>.Success(document.Id);
    }

    private DateTime Now => _timeProvider.GetUtcNow().UtcDateTime;

    // Copies the content, and stops as soon as it goes over the limit: the size given by the browser isn't trusted.
    private async Task<bool> TryCopyWithinLimitAsync(Stream content, MemoryStream file, CancellationToken cancellationToken)
    {
        var chunk = new byte[81_920];
        int read;
        while ((read = await content.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (file.Length + read > MaxUploadSize)
            {
                return false;
            }

            file.Write(chunk, 0, read);
        }

        return true;
    }

    private static bool IsPdf(MemoryStream file) =>
        file.Length >= PdfSignature.Length && file.GetBuffer().AsSpan(0, PdfSignature.Length).SequenceEqual(PdfSignature);
}
