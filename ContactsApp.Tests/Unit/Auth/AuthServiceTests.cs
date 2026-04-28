using ContactsApp.Application.DTOs.Auth;
using ContactsApp.Application.Interfaces;
using ContactsApp.Application.Services;
using ContactsApp.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ContactsApp.Tests.Unit.Auth;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IAuthRepository> _authRepositoryMock = null!;
    private Mock<ITokenService> _tokenServiceMock = null!;
    private Mock<ILogger<AuthService>> _loggerMock = null!;
    private IAuthService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _authRepositoryMock = new Mock<IAuthRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _sut = new AuthService(
            _authRepositoryMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object);
    }

    // ------------------------------------------------------------------ Register

    [Test]
    public async Task RegisterAsync_WhenValidRequest_ShouldReturnAuthResponse()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!",
            FirstName = "Alice",
            LastName = "Dupont"
        };

        var userClaims = new UserTokenClaimsDto
        {
            Id = "user-1",
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        _authRepositoryMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((UserTokenClaimsDto?)null);

        _authRepositoryMock
            .Setup(x => x.CreateUserAsync(dto))
            .ReturnsAsync(userClaims);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(userClaims))
            .Returns(("jwt-token-value", DateTime.UtcNow.AddMinutes(60)));

        // Act
        var result = await _sut.RegisterAsync(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(dto.Email));
        Assert.That(result.FirstName, Is.EqualTo(dto.FirstName));
        Assert.That(result.LastName, Is.EqualTo(dto.LastName));
        Assert.That(result.Token, Is.EqualTo("jwt-token-value"));
    }

    [Test]
    public void RegisterAsync_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!",
            FirstName = "Alice",
            LastName = "Dupont"
        };

        _authRepositoryMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(new UserTokenClaimsDto { Id = "existing", Email = dto.Email });

        // Act & Assert
        Assert.That(
            async () => await _sut.RegisterAsync(dto),
            Throws.TypeOf<ConflictException>());
    }

    [Test]
    public void RegisterAsync_WhenCreateUserFails_ShouldThrowBusinessException()
    {
        // Arrange
        var dto = new RegisterRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!",
            FirstName = "Alice",
            LastName = "Dupont"
        };

        _authRepositoryMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((UserTokenClaimsDto?)null);

        _authRepositoryMock
            .Setup(x => x.CreateUserAsync(dto))
            .ThrowsAsync(new BusinessException("Password too weak"));

        // Act & Assert
        Assert.That(
            async () => await _sut.RegisterAsync(dto),
            Throws.TypeOf<BusinessException>());
    }

    // ------------------------------------------------------------------ Login

    [Test]
    public async Task LoginAsync_WhenValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "alice@example.com",
            Password = "Password1!"
        };

        var userClaims = new UserTokenClaimsDto
        {
            Id = "user-1",
            Email = dto.Email,
            FirstName = "Alice",
            LastName = "Dupont"
        };

        _authRepositoryMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(userClaims);

        _authRepositoryMock
            .Setup(x => x.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync(true);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(userClaims))
            .Returns(("jwt-token-value", DateTime.UtcNow.AddMinutes(60)));

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(dto.Email));
        Assert.That(result.Token, Is.EqualTo("jwt-token-value"));
    }

    [Test]
    public void LoginAsync_WhenUserNotFound_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "unknown@example.com",
            Password = "Password1!"
        };

        _authRepositoryMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((UserTokenClaimsDto?)null);

        // Act & Assert
        Assert.That(
            async () => await _sut.LoginAsync(dto),
            Throws.TypeOf<UnauthorizedException>());
    }

    [Test]
    public void LoginAsync_WhenInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = "alice@example.com",
            Password = "WrongPassword"
        };

        var userClaims = new UserTokenClaimsDto
        {
            Id = "user-1",
            Email = dto.Email,
            FirstName = "Alice",
            LastName = "Dupont"
        };

        _authRepositoryMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(userClaims);

        _authRepositoryMock
            .Setup(x => x.CheckPasswordAsync(dto.Email, dto.Password))
            .ReturnsAsync(false);

        // Act & Assert
        Assert.That(
            async () => await _sut.LoginAsync(dto),
            Throws.TypeOf<UnauthorizedException>());
    }
}
