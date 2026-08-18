using WMS.Domain.Models;

namespace WMS.Domain.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetActiveTokenAsync(Guid userId, string tokenHash, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetActiveTokenWithUserAndRolesAsync(Guid userId, string tokenHash, CancellationToken cancellationToken = default);
    }
}
