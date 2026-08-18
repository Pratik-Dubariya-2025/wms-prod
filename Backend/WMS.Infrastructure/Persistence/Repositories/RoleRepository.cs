using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class RoleRepository(AppDbContext context) : Repository<Role>(context), IRoleRepository
    {
    }
}
