using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zajednica.Community.Core.UseCases;

namespace Zajednica.Community.Infrastructure;

public sealed class MembershipMuteExpiryWorker(
    IServiceScopeFactory scopes,
    ILogger<MembershipMuteExpiryWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                scope.ServiceProvider.GetRequiredService<MuteExpiryService>().EndExpired();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ending expired mutes failed; retrying on the next tick.");
            }
        }
    }
}
