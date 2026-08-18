using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class DepartmentRepository(AppDbContext dbContext) : Repository<Department>(dbContext), IDepartmentRepository
    {
    }
}
