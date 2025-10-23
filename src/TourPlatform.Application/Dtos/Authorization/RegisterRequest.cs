namespace TourPlatform.Application.DTOS.Authorization;

public record RegisterRequest(string Username, string Password, string Role, int? TourOperatorId = null);
