using Zajednica.Community.Api.Internal;
using Zajednica.Community.Api.Internal.Dto;
using Zajednica.Community.Core.Domain.RepositoryInterfaces;
using Zajednica.Community.Core.Mappers;

namespace Zajednica.Community.Core.UseCases.Internal;

public sealed class InternalMembershipDirectoryService(IMembershipRepository membershipRepository)
    : IInternalMembershipDirectoryService
{
    public IReadOnlyList<InternalMembershipAccountIdDto> GetAccountIdsByMembershipIds(IReadOnlyCollection<Guid> membershipIds)
    {
        if (membershipIds.Count == 0)
            return [];

        return membershipRepository.GetManyByIds(membershipIds)
            .Select(m => m.ToAccountDto())
            .ToList();
    }
}
