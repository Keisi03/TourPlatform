namespace TourPlatform.Infrastructure.Repositories;

public interface ISeasonRepository
{
    Task<int> GetOrCreateAsync(string seasonCode, CancellationToken ct);
}
