using MediatR;
using Microsoft.Extensions.Logging;
using WMS.Application.Common.Interfaces;

namespace WMS.Application.Common.Behaviours
{
    public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<TRequest> _logger;
        private readonly ICurrentUserService _currentUserService;

        public LoggingBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUserService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId?.ToString() ?? "Anonymous";
            var username = _currentUserService.Username ?? "Anonymous";

            _logger.LogInformation("WMS Request: {Name} | User: {Username} ({UserId}) | RequestData: {@Request}",
                requestName, username, userId, request);

            try
            {
                var response = await next();
                _logger.LogInformation("WMS Request Succeeded: {Name} | User: {Username} ({UserId})",
                    requestName, username, userId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WMS Request Failed: {Name} | User: {Username} ({UserId}) | Error: {Message}",
                    requestName, username, userId, ex.Message);
                throw;
            }
        }
    }
}
