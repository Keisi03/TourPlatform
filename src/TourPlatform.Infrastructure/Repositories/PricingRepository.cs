using Microsoft.Extensions.Logging;
using TourPlatform.Domain.Entities;
using TourPlatform.Infrastructure.Entities;

namespace TourPlatform.Infrastructure.Repositories
{
    public class PricingRepository : IPricingRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PricingRepository> _logger;

        public PricingRepository(AppDbContext db, ILogger<PricingRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task BulkInsertAsync(IEnumerable<Pricingrecord> records, CancellationToken ct)
        {
            try
            {
                await _db.Pricingrecords.AddRangeAsync(records, ct);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk insert for pricing records");
                throw;
            }
        }
    }
}
