using ContactsApp.Application.DTOs.Auth;
using ContactsApp.Application.Interfaces;
using ContactsApp.Domain.Exceptions;
using ContactsApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace ContactsApp.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<User> _userManager;

    public AuthRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserTokenClaimsDto?> FindByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return null;

        return new UserTokenClaimsDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }

    public async Task<UserTokenClaimsDto> CreateUserAsync(RegisterRequestDto dto)
    {
        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new BusinessException(string.Join(" ", errors));
        }

        return new UserTokenClaimsDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }

    public async Task<bool> CheckPasswordAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return false;

        return await _userManager.CheckPasswordAsync(user, password);
    }
}
