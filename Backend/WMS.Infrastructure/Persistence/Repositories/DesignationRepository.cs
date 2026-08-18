using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class DesignationRepository(AppDbContext dbContext) : Repository<Designation>(dbContext), IDesignationRepository
    {
    }
}
