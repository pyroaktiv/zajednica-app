using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zajednica.Feed.Core.UseCases.Intents;

namespace Zajednica.Feed.Infrastructure;

public sealed class IntentClosingWorker(IServiceScopeFactory scopes, ILogger<IntentClosingWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                scope.ServiceProvider.GetRequiredService<IntentClosingService>().CloseDue();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Closing due intents failed; retrying on the next tick.");
            }
        }
    }
}
