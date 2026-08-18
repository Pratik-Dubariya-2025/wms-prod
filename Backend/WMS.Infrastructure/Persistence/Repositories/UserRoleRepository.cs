using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class UserRoleRepository(AppDbContext context) : Repository<UserRole>(context), IUserRoleRepository
    {
    }
}
