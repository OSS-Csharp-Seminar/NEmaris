namespace NEmaris.Application.DTOs;

public record RegisterRequestDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    string Role
);