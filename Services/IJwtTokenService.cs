using GameStoreWeb.Models;
using System.Security.Claims;

namespace GameStoreWeb.Services;

public interface IJwtTokenService
{
    Task<string> CreateToken(ApplicationUser user);
    RefreshToken GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
