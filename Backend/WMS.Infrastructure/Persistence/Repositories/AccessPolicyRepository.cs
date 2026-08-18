using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class AccessPolicyRepository(AppDbContext context) : Repository<AccessPolicy>(context), IAccessPolicyRepository
    {
    }
}
