using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class PermissionRepository(AppDbContext context) : Repository<Permission>(context), IPermissionRepository
    {
    }
}
