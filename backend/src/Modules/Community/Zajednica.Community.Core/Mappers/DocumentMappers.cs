using Zajednica.Community.Api.Dto.Documents;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Core.Mappers;

public static class DocumentMappers
{
    public static DocumentDto ToDto(this Document document) =>
        new(document.Id, document.Name,
            $"api/communities/{document.CommunityId}/documents/{document.Id}/content",
            document.PostedByMembershipId, document.Date);
}
