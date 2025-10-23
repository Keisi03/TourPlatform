using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TourPlatform.Domain.Entities;
using TourPlatform.Infrastructure.Entities;

namespace TourPlatform.Infrastructure.Repositories
{
    public class TourOperatorRepository : ITourOperatorRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<TourOperatorRepository> _logger;

        public TourOperatorRepository(AppDbContext db, ILogger<TourOperatorRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Touroperator?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _db.Touroperators
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, ct);
        }

        public async Task<bool> UserOwnsOperatorAsync(int userId, int tourOperatorId, CancellationToken ct)
        {

            return await _db.Touroperators
                        .AnyAsync(t => t.Id == tourOperatorId
                        && t.Users.Any(u => u.Id == userId), ct);
        }
    }
}
