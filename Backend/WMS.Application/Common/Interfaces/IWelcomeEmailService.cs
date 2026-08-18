using System.Threading;
using System.Threading.Tasks;

namespace WMS.Application.Common.Interfaces
{
    /// <summary>
    /// Renders and sends the "welcome / account created" email. Extracted out of
    /// the invite-user handler so the handler is not responsible for template
    /// loading and message composition (SRP).
    /// </summary>
    public interface IWelcomeEmailService
    {
        Task SendWelcomeEmailAsync(
            string toEmail,
            string firstName,
            string username,
            string password,
            CancellationToken cancellationToken = default);
    }
}
