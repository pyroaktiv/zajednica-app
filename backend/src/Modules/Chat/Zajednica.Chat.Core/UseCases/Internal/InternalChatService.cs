using Zajednica.Chat.Api.Internal;
using Zajednica.Chat.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Chat.Core.UseCases.Internal;

public sealed class InternalChatService(IChatRepository chatRepository) : IInternalChatService
{
    public void DeleteTemporaryChats(Guid communityId, Guid membershipId) =>
        chatRepository.RemoveTemporary(communityId, membershipId);
}
