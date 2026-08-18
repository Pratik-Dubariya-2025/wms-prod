using MediatR;
using WMS.Application.Common.Models;

namespace WMS.Application.Features.Auth.Commands
{
    public class ResetPasswordCommand : IRequest<ApiResponse<string>>
    {
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
        public string? CookieToken { get; set; }
    }
}
