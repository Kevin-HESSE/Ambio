namespace Ambio.Application.CvDocuments;

/// <summary>Error messages returned by <see cref="ICvDocumentService"/>.</summary>
public static class CvDocumentErrors
{
    public const string NotFound = "This CV doesn't exist anymore.";

    public const string FileMissing = "The file of this CV is missing from the storage.";

    public const string ApplicationNotFound = "This application doesn't exist anymore.";

    public const string NotPdf = "This file isn't a PDF. Choose the PDF you sent.";

    public const string TooLarge = "This file is too large. Choose a PDF within the size limit.";

    public const string UploadFailed = "The CV couldn't be saved. Try again.";
}
