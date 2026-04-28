namespace ContactsApp.Application.DTOs.Auth;

public record AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
