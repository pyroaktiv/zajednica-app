using Zajednica.Feed.Api.Internal.Dto;

namespace Zajednica.Feed.Api.Internal;

public interface IInternalHelpRequestService
{
    HelpRequestInfoDto? Get(Guid communityId, Guid helpRequestId);
}
