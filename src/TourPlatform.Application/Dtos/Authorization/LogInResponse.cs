namespace TourPlatform.Application.Dtos.Authorization;

public record LoginResponse(
 string Token,
 string Jti
);
