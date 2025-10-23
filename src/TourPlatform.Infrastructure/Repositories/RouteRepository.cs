using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TourPlatform.Domain.Entities;
using TourPlatform.Infrastructure.Entities;

namespace TourPlatform.Infrastructure.Repositories;

public class RouteRepository : IRouteRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<RouteRepository> _logger;

    public RouteRepository(AppDbContext db, ILogger<RouteRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> GetOrCreateAsync(string routeCode, int tourOperatorId, CancellationToken ct)
    {
        try
        {
            var existing = await _db.Routes
           .Where(r => r.Routecode == routeCode && r.Touroperatorid == tourOperatorId)
           .Select(r => r.Id)
           .FirstOrDefaultAsync(ct);

            if (existing != 0)    return existing;

            var route = new Route
            {
                Routecode = routeCode,
                Touroperatorid = tourOperatorId
            };

            _db.Routes.Add(route);
            await _db.SaveChangesAsync(ct);

            return route.Id;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error in GetOrCreateAsync for route {Code} and operator {OperatorId}", routeCode, tourOperatorId);
            throw;
        }
       
    }
}
