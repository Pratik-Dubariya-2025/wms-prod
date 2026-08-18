using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class LeaveRequestRepository(AppDbContext context) : Repository<LeaveRequest>(context), ILeaveRequestRepository
    {
    }
}
