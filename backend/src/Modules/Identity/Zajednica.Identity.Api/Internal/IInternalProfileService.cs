using Zajednica.Identity.Api.Internal.Dto;

namespace Zajednica.Identity.Api.Internal;

public interface IInternalProfileService
{
    string? GetUsername(Guid accountId);
    IReadOnlyList<InternalUsernameDto> GetUsernames(IReadOnlyCollection<Guid> accountIds);

    InternalProfileDto? GetProfile(Guid accountId);
    IReadOnlyList<InternalProfileDto> GetProfiles(IReadOnlyCollection<Guid> accountIds);
}
