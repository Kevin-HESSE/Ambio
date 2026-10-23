using System.Linq.Expressions;

using Ambio.Domain.JobApplications;

namespace Ambio.Infrastructure.JobApplications;

/// <summary>Predicates on the applications shared by the services, translated to SQL by EF Core.</summary>
internal static class JobApplicationQueries
{
    /// <summary>The applications sent to the company: to one of its offers, or spontaneously.</summary>
    public static Expression<Func<JobApplication, bool>> OfCompany(Guid companyId) =>
        a => a is OfferApplication
            ? ((OfferApplication)a).JobOffer.CompanyId == companyId
            : ((SpontaneousApplication)a).CompanyId == companyId;
}
