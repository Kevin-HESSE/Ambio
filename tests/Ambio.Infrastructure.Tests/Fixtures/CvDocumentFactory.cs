using Ambio.Domain.CvDocuments;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>CVs to save straight through the DbContext, without the service or any file.</summary>
public static class CvDocumentFactory
{
    /// <summary>Uploaded CVs of the application. Save their application first.</summary>
    public static Faker<CvDocument> GetGenerator(Guid jobApplicationId) =>
        new Faker<CvDocument>()
            .CustomInstantiator(faker => CvDocument.Upload(
                jobApplicationId,
                $"CV_{faker.Name.LastName()}.pdf",
                faker.Random.Long(10_000, 5_000_000),
                faker.Date.Recent().ToUniversalTime()));
}
