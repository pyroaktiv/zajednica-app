using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zajednica.BuildingBlocks.Core.Notifications;
using Zajednica.Identity.Core.Infrastructural.RepositoryInterfaces;
using Zajednica.Identity.Infrastructure.Devices;

namespace Zajednica.Identity.Infrastructure.Notifications;

public sealed class ExpoPushNotificationSender : INotificationSender
{
    private const string ExpoPushEndpoint = "https://exp.host/--/api/v2/push/send";

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IDeviceTokenRepository _deviceTokens;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExpoPushNotificationSender> _logger;
    private readonly string? _accessToken;

    public ExpoPushNotificationSender(
        IDeviceTokenRepository deviceTokens,
        IHttpClientFactory httpClientFactory,
        ILogger<ExpoPushNotificationSender> logger,
        IConfiguration configuration)
    {
        _deviceTokens = deviceTokens;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _accessToken = configuration["Push:AccessToken"];
    }

    public void Send(NotificationRequest request)
    {
        if (request.RecipientAccountIds.Count == 0)
            return;

        var tokens = _deviceTokens.TokensFor(request.RecipientAccountIds.Distinct().ToList())
            .Where(IsExpoToken)
            .ToList();
        if (tokens.Count == 0)
            return;

        var delivery = Delivery(request.Channel);
        var messages = tokens.Select(token => new ExpoMessage(
            token, request.Title, request.Body, delivery.ChannelId, delivery.Priority, delivery.InterruptionLevel,
            DataOf(request.Target))).ToList();

        _ = Task.Run(() => Post(messages));
    }

    private async Task Post(IReadOnlyList<ExpoMessage> messages)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            if (!string.IsNullOrWhiteSpace(_accessToken))
                client.DefaultRequestHeaders.Authorization = new("Bearer", _accessToken);

            var response = await client.PostAsJsonAsync(ExpoPushEndpoint, messages, JsonOptions);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Expo push failed with status {Status}.", (int)response.StatusCode);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Expo push request threw.");
        }
    }

    private static bool IsExpoToken(string token) =>
        token.StartsWith("ExponentPushToken[") || token.StartsWith("ExpoPushToken[");

    private static (string ChannelId, string Priority, string InterruptionLevel) Delivery(NotificationChannel channel) =>
        channel switch
        {
            NotificationChannel.Emergency => ("emergency", "high", "timeSensitive"),
            NotificationChannel.Management => ("management", "high", "timeSensitive"),
            NotificationChannel.Chat => ("chat", "high", "active"),
            _ => ("posts", "default", "active")
        };

    private static IReadOnlyDictionary<string, string>? DataOf(NotificationTarget? target) =>
        target is null
            ? null
            : new Dictionary<string, string>
            {
                ["kind"] = target.Kind,
                ["id"] = target.EntityId.ToString(),
                ["communityId"] = target.CommunityId.ToString()
            };

    private sealed record ExpoMessage(
        string To, string Title, string Body, string ChannelId, string Priority, string InterruptionLevel,
        IReadOnlyDictionary<string, string>? Data);
}
