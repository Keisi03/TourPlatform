using TourPlatform.Application.Dtos.Authorization;
using TourPlatform.Application.DTOS.Authorization;
using TourPlatform.Domain.Entities;

namespace TourPlatform.Application.Contracts;

public interface IAuthorizationFlowService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task LogoutAsync(string jti);
    Task<PlatformUser?> RegisterAsync(RegisterRequest request);
}
