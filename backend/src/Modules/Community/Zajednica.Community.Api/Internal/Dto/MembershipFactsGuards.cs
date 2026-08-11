using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Api.Internal.Dto;

public static class MembershipFactsGuards
{
    public static InternalMembershipFactsDto RequireActive(this InternalMembershipFactsDto? facts)
    {
        if (facts is null)
            throw new ForbiddenException("Not a member of this community.");
        if (!facts.IsActive)
            throw new ForbiddenException("Membership is not active.");

        return facts;
    }

    public static InternalMembershipFactsDto RequireConfirmed(this InternalMembershipFactsDto? facts)
    {
        var active = facts.RequireActive();

        if (!active.IsConfirmed)
            throw new ForbiddenException("Only a confirmed member can do this.");

        return active;
    }

    public static InternalMembershipFactsDto RequireUnconfirmed(this InternalMembershipFactsDto? facts)
    {
        var active = facts.RequireActive();

        if (active.IsConfirmed)
            throw new ForbiddenException("Only an unconfirmed member can do this.");

        return active;
    }

    public static InternalMembershipFactsDto RequireUnmuted(this InternalMembershipFactsDto facts, DateTime now)
    {
        if (facts.MutedUntil is { } until && until > now)
            throw new ForbiddenException("You are muted in this community.");

        return facts;
    }
}
