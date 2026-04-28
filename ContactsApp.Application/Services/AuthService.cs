using ContactsApp.Application.DTOs.Auth;
using ContactsApp.Application.Interfaces;
using ContactsApp.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace ContactsApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IAuthRepository authRepository,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        var existing = await _authRepository.FindByEmailAsync(dto.Email);
        if (existing is not null)
            throw new ConflictException($"Email '{dto.Email}' is already registered.");

        var userClaims = await _authRepository.CreateUserAsync(dto);

        var (token, expiresAt) = _tokenService.GenerateToken(userClaims);

        _logger.LogInformation("User registered: {Email}", dto.Email);

        return new AuthResponseDto
        {
            Token = token,
            Email = userClaims.Email,
            FirstName = userClaims.FirstName,
            LastName = userClaims.LastName,
            ExpiresAt = expiresAt
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _authRepository.FindByEmailAsync(dto.Email);
        if (user is null)
            throw new UnauthorizedException("Invalid email or password.");

        var passwordValid = await _authRepository.CheckPasswordAsync(dto.Email, dto.Password);
        if (!passwordValid)
            throw new UnauthorizedException("Invalid email or password.");

        var (token, expiresAt) = _tokenService.GenerateToken(user);

        _logger.LogInformation("User logged in: {Email}", dto.Email);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ExpiresAt = expiresAt
        };
    }
}
