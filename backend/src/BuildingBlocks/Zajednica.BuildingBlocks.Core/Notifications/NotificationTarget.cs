namespace Zajednica.BuildingBlocks.Core.Notifications;

public record NotificationTarget(string Kind, Guid EntityId, Guid CommunityId);
