using System.Security.Claims;
using System.Security.Principal;
using TourPlatform.Domain.Entities;
using TourPlatform.Infrastructure.Entities;

namespace TourPlatform.Infrastructure.Authorization;
public interface IJwtTokenService
{
        Task<(string Token, string Jti)> GenerateTokenAsync(PlatformUser user);
        ClaimsPrincipal? ValidateToken(string token);
        Task RevokeTokenAsync(string jti);
        Task<bool> IsTokenRevokedAsync(string jti);
}
