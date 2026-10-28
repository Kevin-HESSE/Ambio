namespace Ambio.Application.CvDocuments.Dtos;

/// <summary>A CV sent with an application: the name of its file, its size in bytes, and when it was uploaded.</summary>
public record CvDocumentDto(Guid Id, string FileName, long Size, DateTime UploadedAt);
