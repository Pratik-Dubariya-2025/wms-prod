using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class InvoiceRepository(AppDbContext context) : Repository<Invoice>(context), IInvoiceRepository
    {
    }
}
