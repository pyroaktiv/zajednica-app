using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Dto.HelpChats;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Core.Domain;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Feed.Api.Internal;

namespace Zajednica.Chat.Core.UseCases.HelpChats;

public sealed class HelpRequestChatService(
    IChatRepository chats,
    IInternalHelpRequestService helpRequests,
    IInternalMembershipService memberships,
    ChatAccess access,
    ChatPresenter presenter,
    ChatNotifier notifier) : IHelpRequestChatService
{
    public ChatDetailsDto Respond(Guid accountId, Guid communityId, Guid helpRequestId)
    {
        var helper = access.RequireConfirmed(accountId, communityId);

        var existing = chats.GetActiveHelp(helpRequestId, helper.MembershipId);
        if (existing is not null)
            return presenter.Details(existing, helper.MembershipId);

        var helpRequest = helpRequests.Get(communityId, helpRequestId)
            ?? throw new NotFoundException("Help request not found in this community.");

        if (helpRequest.Closed)
            throw new EntityValidationException("This help request is closed for further responses.");
        if (helpRequest.AuthorMembershipId == helper.MembershipId)
            throw new EntityValidationException("The author of a help request cannot respond to it.");

        var chat = new HelpRequestChat(communityId, helpRequestId, helpRequest.AuthorMembershipId,
            helper.MembershipId, !chats.HasResponded(helpRequestId, helper.MembershipId), DateTime.UtcNow);
        chats.Add(chat);

        notifier.Responded(chat);

        return presenter.Details(chat, helper.MembershipId);
    }

    public ChatDetailsDto ConcludeWithReward(Guid accountId, Guid communityId, Guid chatId,
        ConcludeWithRewardRequest request)
    {
        var (me, chat) = Require(accountId, communityId, chatId);

        return Settle(chat, me.MembershipId, chat.ConcludeWithReward(me.MembershipId, request.Stars));
    }

    public ChatDetailsDto ConcludeWithoutReward(Guid accountId, Guid communityId, Guid chatId)
    {
        var (me, chat) = Require(accountId, communityId, chatId);

        return Settle(chat, me.MembershipId, chat.ConcludeWithoutReward(me.MembershipId));
    }

    public ChatDetailsDto Resign(Guid accountId, Guid communityId, Guid chatId)
    {
        var (me, chat) = Require(accountId, communityId, chatId);

        chat.Resign(me.MembershipId);

        return Settle(chat, me.MembershipId, 0);
    }

    private ChatDetailsDto Settle(HelpRequestChat chat, Guid actorMembershipId, int stars)
    {
        chats.Update(chat);

        if (stars > 0)
            memberships.AddStars(chat.HelperMembershipId, stars);

        notifier.Concluded(chat, stars);

        return presenter.Details(chat, actorMembershipId);
    }

    private (MembershipContextDto Me, HelpRequestChat Chat) Require(Guid accountId, Guid communityId, Guid chatId)
    {
        var me = access.RequireConfirmed(accountId, communityId);

        if (access.RequireChat(communityId, chatId, me.MembershipId) is not HelpRequestChat chat)
            throw new EntityValidationException("Only a help request chat has a conclusion.");

        return (me, chat);
    }
}
