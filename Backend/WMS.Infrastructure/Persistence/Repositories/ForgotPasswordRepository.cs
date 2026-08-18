using WMS.Domain.Interfaces.Repositories;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Repositories
{
    public class ForgotPasswordRepository(AppDbContext dbContext) : Repository<ForgotPassword>(dbContext), IForgotPasswordRepository 
    {
    }
}
