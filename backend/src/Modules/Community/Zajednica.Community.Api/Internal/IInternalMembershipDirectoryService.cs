using Zajednica.Community.Api.Internal.Dto;

namespace Zajednica.Community.Api.Internal;

public interface IInternalMembershipDirectoryService
{
    IReadOnlyList<MemberAccountDto> GetAccounts(IReadOnlyCollection<Guid> membershipIds);
}
