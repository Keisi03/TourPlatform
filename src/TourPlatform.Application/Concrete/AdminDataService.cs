using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using TourPlatform.Application.Contracts;
using TourPlatform.Application.DTOS.FileProcess;
using TourPlatform.Domain.Entities;
using TourPlatform.Infrastructure.Entities;

namespace TourPlatform.Application.Concrete;

public class AdminDataService : IAdminDataService
{
    private readonly AppDbContext _db;
    private readonly IDatabase _redis;

    public AdminDataService(AppDbContext db, IConnectionMultiplexer redis)
    {
        _db = db;
        _redis = redis.GetDatabase();
    }

    public async Task<PagedResult<Pricingrecord>> GetToursDataPerOperatorAsync(
        int tourOperatorId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        string cacheKey = $"admin:pricing:{tourOperatorId}:page{page}:size{pageSize}";

        var cached = await _redis.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<PagedResult<Pricingrecord>>(cached)!;
        }

        var query = _db.Pricingrecords
            .Include(p => p.Route)
            .Include(p => p.Season)
            .Where(p => p.Route.Touroperatorid == tourOperatorId)
            .OrderBy(p => p.Recorddate);

        var totalRecords = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        var result = new PagedResult<Pricingrecord>
        {
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            Items = items
        };

        var serialized = JsonSerializer.Serialize(result);
        await _redis.StringSetAsync(cacheKey, serialized, TimeSpan.FromMinutes(5));

        return result;
    }
}
