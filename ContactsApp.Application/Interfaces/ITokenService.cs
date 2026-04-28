using ContactsApp.Application.DTOs.Auth;

namespace ContactsApp.Application.Interfaces;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateToken(UserTokenClaimsDto claims);
}
