namespace TourPlatform.Infrastructure.Repositories;

public interface IRouteRepository
{
    Task<int> GetOrCreateAsync(string routeCode, int tourOperatorId, CancellationToken ct);
}
