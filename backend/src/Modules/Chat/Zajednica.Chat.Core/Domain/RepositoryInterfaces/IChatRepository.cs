using Zajednica.BuildingBlocks.Core.UseCases;

namespace Zajednica.Chat.Core.Domain.RepositoryInterfaces;

public interface IChatRepository
{
    void Add(Chat chat);
    void Update(Chat chat);

    Chat? Get(Guid id);
    CursorPage<TChat, PageCursor> GetPage<TChat>(Guid communityId, Guid membershipId, PageCursor? before, int limit)
        where TChat : Chat;

    DirectChat? GetDirect(Guid communityId, Guid membershipId, Guid otherMembershipId);
    TemporaryChat? GetTemporary(Guid communityId, Guid uncertifiedMembershipId, Guid issuerMembershipId);
    HelpRequestChat? GetActiveHelp(Guid helpRequestId, Guid helperMembershipId);
    bool HasResponded(Guid helpRequestId, Guid helperMembershipId);
    void RemoveTemporary(Guid communityId, Guid membershipId);

    CursorPage<Message, PageCursor> GetMessagePage(Guid chatId, PageCursor? before, int limit);
}
