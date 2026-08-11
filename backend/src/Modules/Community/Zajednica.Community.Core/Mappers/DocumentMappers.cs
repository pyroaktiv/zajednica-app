using Zajednica.BuildingBlocks.Core.Storage;
using Zajednica.Community.Api.Dto.Documents;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Core.Mappers;

public static class DocumentMappers
{
    public static DocumentDto ToDto(this Document document, IFileUrlMapper urls) =>
        new(document.Id, document.Name, urls.ToUrl(document.Url)!, document.PostedByMembershipId, document.Date);
}
