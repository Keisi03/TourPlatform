using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TourPlatform.Application.Contracts;
using TourPlatform.Application.Dtos.Authorization;
using TourPlatform.Application.DTOS.Authorization;
using TourPlatform.Domain.Entities;
using TourPlatform.Infrastructure.Authorization;
using TourPlatform.Infrastructure.Entities;

namespace TourPlatform.Application.Concrete;

public class AuthorizationFlowService : IAuthorizationFlowService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AuthorizationFlowService> _logger;

    public AuthorizationFlowService(IJwtTokenService jwtTokenService, AppDbContext dbContext, ILogger<AuthorizationFlowService> logger)
    {
        _jwtTokenService = jwtTokenService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null)
            {
                _logger.LogWarning("Login failed for user {Username}: user not found", request.Username);
                return null;
            }

            if (!VerifyPassword(request.Password, user.Passwordhash))
            {
                _logger.LogWarning("Login failed for user {Username}: invalid password", request.Username);
                return null;
            }

            var (token, jti) = await _jwtTokenService.GenerateTokenAsync(new PlatformUser(
                user.Username, request.Password, user.Role
            ));

            return new LoginResponse(token, jti);
        }
        catch (Exception ex) {
            _logger.LogError("User {Username} not logged due to error", ex);

            return null;
        }

    }

    public async Task LogoutAsync(string jti)
    {
        _logger.LogInformation("Revoking token with JTI {Jti}", jti);
        try
        {
            await _jwtTokenService.RevokeTokenAsync(jti);

        }
        catch(Exception ex)
        {
            _logger.LogError("Token {Jti} Was not revoked successfully", jti);
        }
        _logger.LogInformation("Token {Jti} revoked successfully", jti);

    }

    public async Task<PlatformUser?> RegisterAsync(RegisterRequest request)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Username == request.Username))
        {
            _logger.LogWarning("Registration failed: username {Username} already exists", request.Username);
            return null;
        }

        try
        {
            var passwordHash = HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Passwordhash = passwordHash,
                Role = request.Role,
                Touroperatorid = request.TourOperatorId
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("User {Username} registered successfully with role {Role}", user.Username, user.Role);

            return new PlatformUser(user.Username, request.Password, user.Role);
        }
        catch (Exception ex)
        {
            _logger.LogError("User {Username}failed to register", ex);

            return new PlatformUser("","","");
        }

    }

    private static byte[] HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        return sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    }

    private static bool VerifyPassword(string password, byte[] storedHash)
    {
        var hash = HashPassword(password);
        return hash.SequenceEqual(storedHash);
    }
}
