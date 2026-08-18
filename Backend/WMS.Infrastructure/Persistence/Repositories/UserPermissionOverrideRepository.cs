using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class UserPermissionOverrideRepository(AppDbContext dbContext) : Repository<UserPermissionOverride>(dbContext), IUserPermissionOverrideRepository
    {
    }
}
