using Zajednica.Community.Api.Internal.Dto;

namespace Zajednica.Community.Api.Internal;

public interface IInternalMembershipDirectoryService
{
    IReadOnlyList<InternalMembershipAccountIdDto> GetAccountIdsByMembershipIds(IReadOnlyCollection<Guid> membershipIds);
}
