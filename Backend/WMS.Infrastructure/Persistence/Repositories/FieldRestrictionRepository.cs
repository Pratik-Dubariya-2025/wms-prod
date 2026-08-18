using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class FieldRestrictionRepository(AppDbContext context) : Repository<FieldRestriction>(context), IFieldRestrictionRepository
    {
    }
}
