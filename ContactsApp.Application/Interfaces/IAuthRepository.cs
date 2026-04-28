using ContactsApp.Application.DTOs.Auth;

namespace ContactsApp.Application.Interfaces;

public interface IAuthRepository
{
    Task<UserTokenClaimsDto?> FindByEmailAsync(string email);
    Task<UserTokenClaimsDto> CreateUserAsync(RegisterRequestDto dto);
    Task<bool> CheckPasswordAsync(string email, string password);
}
