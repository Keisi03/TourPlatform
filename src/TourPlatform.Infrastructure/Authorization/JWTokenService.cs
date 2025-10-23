using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TourPlatform.Domain.Entities;
using TourPlatform.Infrastructure.Authorization.Settings;

namespace TourPlatform.Infrastructure.Authorization;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisSettings _redisSettings;

    public JwtTokenService(
        IOptions<JwtSettings> jwtOptions,
        IOptions<RedisSettings> redisOptions,
        IConnectionMultiplexer redis)
    {
        _jwtSettings = jwtOptions.Value;
        _redisSettings = redisOptions.Value;
        _redis = redis;
    }

    public async Task<(string Token, string Jti)> GenerateTokenAsync(PlatformUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim(JwtRegisteredClaimNames.Jti, jti)
    };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var db = _redis.GetDatabase();
        await db.StringSetAsync($"issued:{jti}", true, TimeSpan.FromMinutes(_jwtSettings.ExpireMinutes));

        return (tokenString, jti);
    }



    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

        try
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task RevokeTokenAsync(string jti)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(
            $"revoked:{jti}",
            true,
            TimeSpan.FromMinutes(_redisSettings.JtiExpiryMinutes)
        );
    }
    public async Task<bool> IsTokenRevokedAsync(string jti)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync($"revoked:{jti}");
        return value.HasValue;
    }
}