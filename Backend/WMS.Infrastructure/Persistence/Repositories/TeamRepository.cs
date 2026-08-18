using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class TeamRepository(AppDbContext context) : Repository<Team>(context), ITeamRepository
    {
    }
}
