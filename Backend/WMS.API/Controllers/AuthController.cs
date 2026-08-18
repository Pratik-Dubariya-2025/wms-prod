using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.Common.Interfaces;
using WMS.Application.Common.Models;
using WMS.Application.Features.Auth.Commands;
using WMS.Application.Features.Auth.DTOs;

namespace WMS.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IMediator mediator, ICurrentUserService currentUserService, IPermissionCacheService permissionCacheService) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IPermissionCacheService _permissionCacheService = permissionCacheService;

        #region GET

        [HttpGet("auth-check")]
        [Authorize]
        public IActionResult AuthCheck()
        {
            return Ok(ApiResponse.Success(new { _currentUserService.Email, _currentUserService.UserId, _currentUserService.Roles}));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserPermissions()
        {
            GetCurrentUserPermissionQuery query = new();
            ApiResponse<List<string>> response = await _mediator.Send(query);
            return StatusCode(response.StatusCode, response);
        }

        #endregion


        #region POST method

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand request)
        {
            ApiResponse<AuthResponseDto> response = await _mediator.Send(request);
            if (!response.Succeeded || response.Data == null)
            {            
                return StatusCode(response.StatusCode, response);
            }

            if (response.Data.RequiresMfa)
            {
                return StatusCode(response.StatusCode, ApiResponse<AuthResponseDto>.Success(
                    data: response.Data,
                    message: response.Message,
                    statusCode: response.StatusCode
                ));
            }

            if (!string.IsNullOrEmpty(response.Data.RefreshToken))
            {
                CookieOptions cookieOptions = new()
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                    Expires = response.Data.Expiration
                };
                Response.Cookies.Append("RefreshToken", response.Data.RefreshToken, cookieOptions);
            }

            return StatusCode(response.StatusCode, ApiResponse<string>.Success(
                data: response.Data.AccessToken!,
                message: response.Message,
                statusCode: response.StatusCode
            ));
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (_currentUserService.UserId.HasValue)
            {
                _permissionCacheService.InvalidateUser(_currentUserService.UserId.Value);
            }

            string? refreshToken = Request.Cookies["RefreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                LogoutCommand command = new(refreshToken);
                await _mediator.Send(command);
            }

            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            Response.Cookies.Append("RefreshToken", "", cookieOptions);

            return Ok(ApiResponse.Success("Logged out successfully."));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            string? refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized, 
                    ApiResponse.Failure("Unauthorized access", null, 401)
                ); 
            }

            string authHeader = Request.Headers.Authorization.ToString();
            string accessToken = authHeader.StartsWith("Bearer ")
                ? authHeader[7..]
                : authHeader;

            if (string.IsNullOrEmpty(accessToken))
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponse.Failure("Unauthorized access", null, 401)
                );
            }

            RefreshTokenCommand command = new() { AccessToken = accessToken, RefreshToken = refreshToken };
            ApiResponse<AuthResponseDto> response = await _mediator.Send(command);

            if (!response.Succeeded || response.Data == null)
            {
                return StatusCode(response.StatusCode, response);
            }

            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = response.Data.Expiration
            };
            Response.Cookies.Append("RefreshToken", response.Data.RefreshToken, cookieOptions);

            return StatusCode(response.StatusCode, ApiResponse<string>.Success(
                data: response.Data.AccessToken,
                message: "Token refreshed successfully.",
                statusCode: response.StatusCode
            ));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordCommand request)
        {
            ApiResponse<string> response = await _mediator.Send(request);
            
            if (response.Succeeded && !string.IsNullOrEmpty(response.Data))
            {
                CookieOptions cookieOptions = new()
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30)
                };
                Response.Cookies.Append("ResetToken", response.Data, cookieOptions);
            }
            
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand request)
        {
            request.CookieToken = Request.Cookies["ResetToken"];
            ApiResponse<string> response = await _mediator.Send(request);
            
            if (response.Succeeded)
            {
                Response.Cookies.Delete("ResetToken");
            }
            
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("verify-first-time-login-email")]
        public async Task<IActionResult> VerifyFirstTimeLoginEmail(VerifyFirstTimeLoginEmailCommand request)
        {
            ApiResponse<string> response = await _mediator.Send(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("change-first-time-password")]
        public async Task<IActionResult> ChangeFirstTimePassword(ChangeFirstTimePasswordCommand request)
        {
            ApiResponse<string> response = await _mediator.Send(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            if (_currentUserService.UserId.HasValue)
            {
                _permissionCacheService.InvalidateUser(_currentUserService.UserId.Value);
            }

            LogoutAllCommand command = new();
            var response = await _mediator.Send(command);
            
            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            Response.Cookies.Append("RefreshToken", "", cookieOptions);
            
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("mfa/setup")]
        [Authorize]
        public async Task<IActionResult> SetupMfa()
        {
            SetupMfaCommand command = new();
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("mfa/verify")]
        [Authorize]
        public async Task<IActionResult> VerifyMfa(VerifyMfaCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        #endregion
    }
}

