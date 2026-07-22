namespace Zajednica.Feed.Api.Dto.Intents;

public record OpenIntentRequest(Guid TargetMembershipId, string Text);
