using TourPlatform.Application.DTOS.FileProcess;
using TourPlatform.Domain.Entities;

namespace TourPlatform.Application.Contracts;

public interface IAdminDataService
{
    Task<PagedResult<Pricingrecord>> GetToursDataPerOperatorAsync(int tourOperatorId, int page, int pageSize, CancellationToken ct);
}
