using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class EmployeeProfileRepository(AppDbContext context) : Repository<EmployeeProfile>(context), IEmployeeProfileRepository
    {
    }
}
