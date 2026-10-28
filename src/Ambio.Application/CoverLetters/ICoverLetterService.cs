using Ambio.Application.Common;
using Ambio.Application.CoverLetters.Dtos;

namespace Ambio.Application.CoverLetters;

public interface ICoverLetterService
{
    /// <summary>The cover letters of the application, archived or not, in the order they were written, so the list
    /// doesn't move when one is saved. Empty when the application has none or doesn't exist.</summary>
    public Task<IReadOnlyList<CoverLetterDto>> ListAsync(Guid applicationId, CancellationToken cancellationToken = default);

    /// <summary>Writes a new cover letter for the application and returns its id. Fails with
    /// <see cref="CoverLetterErrors.ApplicationNotFound"/>.</summary>
    public Task<ServiceResult<Guid>> CreateAsync(Guid applicationId, CoverLetterInput input, CancellationToken cancellationToken = default);

    /// <summary>Replaces the title and the text of the letter. Fails with <see cref="CoverLetterErrors.NotFound"/>.</summary>
    public Task<ServiceResult> UpdateAsync(Guid id, CoverLetterInput input, CancellationToken cancellationToken = default);

    /// <summary>Deletes the letter for good: nothing refers to it. Fails with
    /// <see cref="CoverLetterErrors.NotFound"/>.</summary>
    public Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
