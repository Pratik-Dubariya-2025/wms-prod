using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class ProjectMemberRepository(AppDbContext context) : Repository<ProjectMember>(context), IProjectMemberRepository
    {
    }
}
