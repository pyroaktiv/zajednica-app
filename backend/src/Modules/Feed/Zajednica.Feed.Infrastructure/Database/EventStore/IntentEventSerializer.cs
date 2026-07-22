using System.Text.Json;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Feed.Core.Domain.Intents.Events;

namespace Zajednica.Feed.Infrastructure.Database.EventStore;

public static class IntentEventSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Serialize(IntentEvent domainEvent) => JsonSerializer.Serialize(domainEvent, Options);

    public static IntentEvent Deserialize(string payload) =>
        JsonSerializer.Deserialize<IntentEvent>(payload, Options)
        ?? throw new EntityValidationException("A stored intent event could not be read.");
}
