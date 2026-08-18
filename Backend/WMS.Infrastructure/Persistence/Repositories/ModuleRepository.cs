using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class ModuleRepository(AppDbContext context) : Repository<Module>(context), IModuleRepository
    {
    }
}
