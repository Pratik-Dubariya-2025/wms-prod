using MediatR;
using WMS.Application.Common.Models;

namespace WMS.Application.Features.Auth.Commands
{
    public class ForgotPasswordCommand : IRequest<ApiResponse<string>>
    {
        public string Email { get; set; } = null!;
    }
}
