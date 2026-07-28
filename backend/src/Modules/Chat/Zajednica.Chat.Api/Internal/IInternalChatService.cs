namespace Zajednica.Chat.Api.Internal;

public interface IInternalChatService
{
    void DeleteTemporaryChats(Guid communityId, Guid membershipId);
}
