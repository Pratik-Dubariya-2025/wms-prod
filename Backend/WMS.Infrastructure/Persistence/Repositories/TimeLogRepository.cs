using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class TimeLogRepository(AppDbContext context) : Repository<TimeLog>(context), ITimeLogRepository
    {
    }
}
