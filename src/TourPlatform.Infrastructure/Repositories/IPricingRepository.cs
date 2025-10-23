using TourPlatform.Domain.Entities;

namespace TourPlatform.Infrastructure.Repositories;

public interface IPricingRepository
{
    Task BulkInsertAsync(IEnumerable<Pricingrecord> records, CancellationToken cancellationToken);
}
