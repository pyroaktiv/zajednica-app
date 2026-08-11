using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Chat.Api.Dto.Chats;
using Zajednica.Chat.Api.Dto.HelpChats;
using Zajednica.Chat.Api.Public;
using Zajednica.Chat.Core.Domain;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Api.Internal;
using Zajednica.Feed.Api.Internal;

namespace Zajednica.Chat.Core.UseCases.HelpChats;

public sealed class HelpRequestChatService(
    IChatRepository chats,
    IInternalHelpRequestService helpRequests,
    IInternalMembershipCommandService stars,
    ChatRequirementsService requirementsService,
    ChatPresenterService presenterService,
    ChatNotifier notifier) : IHelpRequestChatService
{
    public ChatDetailsDto Respond(Guid accountId, Guid communityId, Guid helpRequestId)
    {
        var helperMembershipId = requirementsService.RequireUnmutedConfirmed(accountId, communityId);

        var existing = chats.GetActiveHelp(helpRequestId, helperMembershipId);
        if (existing is not null)
            return presenterService.Details(existing, helperMembershipId);

        var authorMembershipId = helpRequests.RequireRespondableBy(communityId, helpRequestId, helperMembershipId);

        var chat = new HelpRequestChat(communityId, helpRequestId, authorMembershipId,
            helperMembershipId, !chats.HasResponded(helpRequestId, helperMembershipId), DateTime.UtcNow);
        chats.Add(chat);

        notifier.Responded(chat);

        return presenterService.Details(chat, helperMembershipId);
    }

    public ChatDetailsDto ConcludeWithReward(Guid accountId, Guid communityId, Guid chatId,
        ConcludeWithRewardRequestDto requestDto)
    {
        var (myMembershipId, chat) = Require(accountId, communityId, chatId);

        return Settle(chat, myMembershipId, chat.ConcludeWithReward(myMembershipId, requestDto.Stars));
    }

    public ChatDetailsDto ConcludeWithoutReward(Guid accountId, Guid communityId, Guid chatId)
    {
        var (myMembershipId, chat) = Require(accountId, communityId, chatId);

        return Settle(chat, myMembershipId, chat.ConcludeWithoutReward(myMembershipId));
    }

    public ChatDetailsDto Resign(Guid accountId, Guid communityId, Guid chatId)
    {
        var (myMembershipId, chat) = Require(accountId, communityId, chatId);

        chat.Resign(myMembershipId);

        return Settle(chat, myMembershipId, 0);
    }

    private ChatDetailsDto Settle(HelpRequestChat chat, Guid actorMembershipId, int awardedStars)
    {
        chats.Update(chat);

        if (awardedStars > 0)
            stars.AddStars(chat.HelperMembershipId, awardedStars);

        notifier.Concluded(chat, awardedStars);

        return presenterService.Details(chat, actorMembershipId);
    }

    private (Guid MyMembershipId, HelpRequestChat Chat) Require(Guid accountId, Guid communityId, Guid chatId)
    {
        var myMembershipId = requirementsService.RequireConfirmed(accountId, communityId);

        if (requirementsService.RequireChat(communityId, chatId, myMembershipId) is not HelpRequestChat chat)
            throw new EntityValidationException("Only a help request chat has a conclusion.");

        return (myMembershipId, chat);
    }
}
