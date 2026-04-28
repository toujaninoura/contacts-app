using ContactsApp.Application.DTOs;
using ContactsApp.Application.DTOs.Auth;
using ContactsApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactsApp.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>Register a new user account.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var response = await _authService.RegisterAsync(dto);
        _logger.LogInformation("New user registered: {Email}", dto.Email);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<AuthResponseDto>.Ok(response, "Registration successful."));
    }

    /// <summary>Authenticate and obtain a JWT token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var response = await _authService.LoginAsync(dto);
        _logger.LogInformation("User logged in: {Email}", dto.Email);
        return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Login successful."));
    }
}
