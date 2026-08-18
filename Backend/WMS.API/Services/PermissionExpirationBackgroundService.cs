using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WMS.Application.Common.Interfaces;
using WMS.Domain.Interfaces;

namespace WMS.API.Services
{
    /// <summary>
    /// Background service that periodically checks for expired user permission overrides.
    /// When an override reaches its expiration date (ExpiresAt <= DateTime.UtcNow):
    /// 1. Soft-deletes the expired override.
    /// 2. Invalidates the user's permission cache.
    /// 3. Pushes the recomputed permissions to the user via SignalR in real time.
    /// </summary>
    public class PermissionExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PermissionExpirationBackgroundService> _logger;
        private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(30);

        public PermissionExpirationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PermissionExpirationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(CheckInterval);

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await CheckAndExpireOverridesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing expired permission overrides.");
                }
            }
        }

        private async Task CheckAndExpireOverridesAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var realtimeService = scope.ServiceProvider.GetRequiredService<IRealtimeNotificationService>();

            var now = DateTime.UtcNow;

            // Find all active overrides that have passed their Expiration date
            var expiredOverrides = await unitOfWork.UserPermissionOverride.GetAllAsync(
                upo => !upo.IsDeleted && upo.ExpiresAt.HasValue && upo.ExpiresAt.Value <= now
            );

            if (expiredOverrides.Count == 0) return;

            var affectedUserIds = expiredOverrides.Select(o => o.UserId).Distinct().ToList();

            foreach (var overrideRecord in expiredOverrides)
            {
                overrideRecord.IsDeleted = true;
                overrideRecord.DeletedBy = "System (ExpirationJob)";
                overrideRecord.DeletedAt = now;
                unitOfWork.UserPermissionOverride.Update(overrideRecord);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Expired {Count} permission overrides for {UserCount} users.", expiredOverrides.Count, affectedUserIds.Count);

            // Notify each affected user over SignalR to update their UI permissions live
            foreach (var userId in affectedUserIds)
            {
                await realtimeService.NotifyUserPermissionsChangedAsync(userId, cancellationToken);
            }
        }
    }
}
