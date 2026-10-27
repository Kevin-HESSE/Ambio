namespace Ambio.Application.CvDocuments.Dtos;

/// <summary>The file of a CV, to send to the browser: the name it was uploaded with, and its content, which the caller
/// disposes.</summary>
public record CvFileDto(string FileName, Stream Content);
