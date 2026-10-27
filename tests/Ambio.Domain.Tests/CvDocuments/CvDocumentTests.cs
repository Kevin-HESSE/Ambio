using Ambio.Domain.CvDocuments;

namespace Ambio.Domain.Tests.CvDocuments;

public class CvDocumentTests
{
    private const string FileName = "CV_Backend_2026-09.pdf";
    private const long Size = 188_416;

    private static readonly DateTime UploadedAt = new(2026, 9, 3, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Upload_GeneratesVersion7Id()
    {
        Assert.Equal(7, Upload().Id.Version);
    }

    [Fact]
    public void Upload_KeepsTheApplicationTheNameAndTheSize()
    {
        var jobApplicationId = Guid.CreateVersion7();

        var document = CvDocument.Upload(jobApplicationId, FileName, Size, UploadedAt);

        Assert.Equal((jobApplicationId, FileName, Size), (document.JobApplicationId, document.FileName, document.Size));
    }

    [Fact]
    public void Upload_IsAnUploadedCv()
    {
        Assert.Equal(CvSource.Uploaded, Upload().Source);
    }

    [Fact]
    public void Upload_NamesTheFileAfterItsIdInTheFolderOfItsYear()
    {
        var document = Upload();

        Assert.Equal($"documents/2026/{document.Id}.pdf", document.StoragePath);
    }

    [Fact]
    public void Upload_FileNameWithSurroundingWhitespace_IsTrimmed()
    {
        Assert.Equal(FileName, Upload(fileName: $"  {FileName}\t").FileName);
    }

    [Fact]
    public void Upload_FileNameNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Upload(fileName: null!));
    }

    [Theory]
    [MemberData(nameof(BlankValues))]
    public void Upload_FileNameBlank_ThrowsArgumentException(string fileName)
    {
        Assert.Throws<ArgumentException>(() => Upload(fileName: fileName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Upload_SizeNotPositive_ThrowsArgumentOutOfRangeException(long size)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Upload(size: size));
    }

    private static CvDocument Upload(string fileName = FileName, long size = Size) =>
        CvDocument.Upload(Guid.CreateVersion7(), fileName, size, UploadedAt);

    public static TheoryData<string> BlankValues =>
    [
        "",
        " ",
        "\t\n",
    ];
}
