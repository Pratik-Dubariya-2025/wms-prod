using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class TaskItemRepository(AppDbContext context) : Repository<TaskItem>(context), ITaskItemRepository
    {
    }
}
