using Ambio.Application.CvDocuments.Dtos;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>CVs sent with an application, as returned by the mocked <c>ICvDocumentService</c>.</summary>
public static class CvDocumentFactory
{
    public static Faker<CvDocumentDto> GetGenerator() =>
        new Faker<CvDocumentDto>()
            .WithRecord()
            .RuleFor(d => d.Id, Guid.CreateVersion7)
            .RuleFor(d => d.FileName, f => $"CV_{f.Name.LastName()}.pdf")
            .RuleFor(d => d.Size, f => f.Random.Long(10_000, 5_000_000))
            .RuleFor(d => d.UploadedAt, f => f.Date.RecentOffset().UtcDateTime);
}
