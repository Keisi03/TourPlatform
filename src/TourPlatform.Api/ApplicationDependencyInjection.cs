using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TourPlatform.Application.Concrete;
using TourPlatform.Application.Contracts;
using TourPlatform.Application.Settings;
using TourPlatform.Infrastructure.Authorization.Settings;
using TourPlatform.Infrastructure.Repositories;

namespace TourPlatform.Api;
public static class ApplicationDependencyInjection
{

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddApiAuthentication(services, configuration);
        AddSettings(services, configuration);
        AddServices(services, configuration);

        return services;
    }


    private static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                   ?? throw new InvalidOperationException("JwtSettings section is missing or invalid.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,

                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var redis = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                    var db = redis.GetDatabase();
                    var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                    if (jti != null)
                    {
                        var isRevoked = await db.StringGetAsync($"revoked:{jti}");
                        if (isRevoked.HasValue)
                        {
                            context.Fail("Token has been revoked.");
                        }
                    }
                },
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }


    private static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthorizationFlowService, AuthorizationFlowService>();
        services.AddScoped<IFileProcessorService, FileProcessorService>();
        services.AddScoped<IPricingRepository, PricingRepository>();
        services.AddScoped<ITourOperatorRepository, TourOperatorRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IAdminDataService, AdminDataService>();
        services.AddScoped<ISeasonRepository, SeasonRepository>();

        return services;
    }

    private static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    { 
        services.Configure<BulkUploadSettings>(configuration.GetSection("BulkUploadSettings"));
        return services;
    }
}
