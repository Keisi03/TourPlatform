using TourPlatform.Domain.Entities;

namespace TourPlatform.Infrastructure.Repositories;

public interface ITourOperatorRepository
{
    Task<Touroperator?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Checks if the given user has access to this tour operator’s data.
    /// </summary>
    Task<bool> UserOwnsOperatorAsync(int userId, int tourOperatorId, CancellationToken ct);
}
