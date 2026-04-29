using ContactsApp.API.Controllers;
using ContactsApp.Application.DTOs;
using ContactsApp.Application.DTOs.Auth;
using ContactsApp.Application.Interfaces;
using ContactsApp.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContactsApp.Tests.Unit.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _authServiceMock = null!;
    private Mock<ILogger<AuthController>> _loggerMock = null!;
    private AuthController _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _sut = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    // ------------------------------------------------------------------ Register

    [Test]
    public async Task Register_WhenValidRequest_ShouldReturn201WithAuthResponse()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!",
            FirstName = "Alice",
            LastName = "Dupont"
        };

        var authResponse = new AuthResponseDto
        {
            Token = "jwt-token-value",
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(dto))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _sut.Register(dto);

        // Assert
        var statusResult = result as ObjectResult;
        Assert.That(statusResult, Is.Not.Null);
        Assert.That(statusResult!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));

        var apiResponse = statusResult.Value as ApiResponse<AuthResponseDto>;
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Success, Is.True);
        Assert.That(apiResponse.Data!.Email, Is.EqualTo("alice@example.com"));
        Assert.That(apiResponse.Data.Token, Is.EqualTo("jwt-token-value"));
    }

    [Test]
    public void Register_WhenEmailAlreadyExists_ShouldPropagateConflictException()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!",
            FirstName = "Alice",
            LastName = "Dupont"
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(dto))
            .ThrowsAsync(new ConflictException("Email already registered."));

        // Act & Assert
        Assert.That(
            async () => await _sut.Register(dto),
            Throws.TypeOf<ConflictException>());
    }

    // ------------------------------------------------------------------ Login

    [Test]
    public async Task Login_WhenValidCredentials_ShouldReturn200WithAuthResponse()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!"
        };

        var authResponse = new AuthResponseDto
        {
            Token = "jwt-token-value",
            Email = dto.Email,
            FirstName = "Alice",
            LastName = "Dupont",
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(dto))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _sut.Login(dto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

        var apiResponse = okResult.Value as ApiResponse<AuthResponseDto>;
        Assert.That(apiResponse, Is.Not.Null);
        Assert.That(apiResponse!.Success, Is.True);
        Assert.That(apiResponse.Data!.Email, Is.EqualTo("alice@example.com"));
        Assert.That(apiResponse.Data.Token, Is.EqualTo("jwt-token-value"));
    }

    [Test]
    public void Login_WhenUserNotFound_ShouldPropagateUnauthorizedException()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "unknown@example.com",
            Password = "Password1!"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(dto))
            .ThrowsAsync(new UnauthorizedException("Invalid credentials."));

        // Act & Assert
        Assert.That(
            async () => await _sut.Login(dto),
            Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public void Login_WhenInvalidPassword_ShouldPropagateUnauthorizedException()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "alice@example.com",
            Password = "WrongPassword"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(dto))
            .ThrowsAsync(new UnauthorizedException("Invalid credentials."));

        // Act & Assert
        Assert.That(
            async () => await _sut.Login(dto),
            Throws.TypeOf<UnauthorizedException>());
    }
}
