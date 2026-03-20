namespace NEmaris.Application.DTOs;

public record RegisterRequestDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken
);

public record RefreshRequestDto(string RefreshToken);

public record RevokeRequestDto(string RefreshToken);