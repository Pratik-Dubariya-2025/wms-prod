using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class PolicyConditionRepository(AppDbContext context) : Repository<PolicyCondition>(context), IPolicyConditionRepository
    {
    }
}
