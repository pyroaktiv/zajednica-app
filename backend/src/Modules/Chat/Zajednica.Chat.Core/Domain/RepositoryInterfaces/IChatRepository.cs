using Zajednica.BuildingBlocks.Core.UseCases;

namespace Zajednica.Chat.Core.Domain.RepositoryInterfaces;

public interface IChatRepository
{
    void Add(Chat chat);
    void Update(Chat chat);

    Chat? Get(Guid id);
    CursorPage<Chat> GetPage(Guid communityId, Guid membershipId, DateTime? before, int limit);

    DirectChat? GetDirect(Guid communityId, Guid membershipId, Guid otherMembershipId);
    TemporaryChat? GetTemporary(Guid communityId, Guid uncertifiedMembershipId, Guid issuerMembershipId);
    HelpRequestChat? GetActiveHelp(Guid helpRequestId, Guid helperMembershipId);
    bool HasResponded(Guid helpRequestId, Guid helperMembershipId);
    void RemoveTemporary(Guid communityId, Guid membershipId);

    CursorPage<Message> GetMessagePage(Guid chatId, DateTime? after, int limit);
}
