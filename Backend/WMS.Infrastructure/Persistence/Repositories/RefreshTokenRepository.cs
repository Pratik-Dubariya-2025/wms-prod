using Microsoft.EntityFrameworkCore;
using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository(AppDbContext context) : Repository<RefreshToken>(context), IRefreshTokenRepository
    {
        public async Task<RefreshToken?> GetActiveTokenAsync(Guid userId, string tokenHash, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.TokenHash == tokenHash, cancellationToken);
        }

        public async Task<RefreshToken?> GetActiveTokenWithUserAndRolesAsync(Guid userId, string tokenHash, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(t => t.User)
                    .ThenInclude(u => u.Designation)
                .Include(t => t.User)
                    .ThenInclude(u => u.Department)
                .Include(t => t.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.TokenHash == tokenHash, cancellationToken);
        }
    }
}
