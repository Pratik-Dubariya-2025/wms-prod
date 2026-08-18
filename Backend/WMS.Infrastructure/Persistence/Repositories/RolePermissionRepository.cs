using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class RolePermissionRepository(AppDbContext context) : Repository<RolePermission>(context), IRolePermissionRepository
    {
    }
}
