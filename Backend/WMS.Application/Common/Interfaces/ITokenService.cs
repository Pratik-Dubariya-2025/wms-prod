using System.Security.Claims;
using WMS.Domain.Models;

namespace WMS.Application.Common.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        string HashToken(string token);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
