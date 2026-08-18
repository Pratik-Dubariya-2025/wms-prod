using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace WMS.API.Hubs
{
    /// <summary>
    /// Maps the authenticated user's ID from JWT claims to SignalR's User identifier.
    /// Checks NameIdentifier, sub, and UserId claims to ensure compatibility regardless
    /// of claim mapping configurations in JwtSecurityTokenHandler or JsonWebTokenHandler.
    /// </summary>
    public class JwtUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var userId = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? connection.User?.FindFirst("sub")?.Value
                ?? connection.User?.FindFirst("UserId")?.Value;

            return userId?.ToLowerInvariant();
        }
    }
}
