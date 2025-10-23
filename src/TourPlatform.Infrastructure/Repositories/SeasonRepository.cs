using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TourPlatform.Domain.Entities;
using TourPlatform.Infrastructure.Entities;

namespace TourPlatform.Infrastructure.Repositories;

public class SeasonRepository : ISeasonRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<SeasonRepository> _logger;

    public SeasonRepository(AppDbContext db, ILogger<SeasonRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> GetOrCreateAsync(string seasonCode, CancellationToken ct)
    {
        try
        {
            var existing = await _db.Seasons
                          .Where(s => s.Seasoncode == seasonCode)
                          .Select(s => s.Id)
                          .FirstOrDefaultAsync(ct);

            if (existing != 0)   return existing;

            var season = new Season { Seasoncode = seasonCode };
            _db.Seasons.Add(season);
            await _db.SaveChangesAsync(ct);

            return season.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateAsync for season {Code}", seasonCode);

            throw;
        }
    }
}
