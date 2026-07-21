using Zajednica.Community.Api.Dto;
using Zajednica.Community.Core.Domain;
using CommunityAggregate = Zajednica.Community.Core.Domain.Community;

namespace Zajednica.Community.Core.Mappers;

public static class CommunityMappers
{
    public static CommunityDto ToDto(this CommunityAggregate community) =>
        new(community.Id,
            community.Name,
            community.Address.ToDto(),
            community.RegistrationNumber?.Value,
            community.TaxId?.Value,
            community.BankAccountNumber,
            community.DateCreated);

    public static CommunitySummaryDto ToSummaryDto(this CommunityAggregate community, Membership membership) =>
        new(community.Id,
            community.Name,
            community.Address.ToDto(),
            membership.IsConfirmed(),
            membership.Roles.Select(r => r.Role.ToString()).ToList());

    public static CommunityQrDto ToQrDto(this CommunityAggregate community) =>
        new(community.Id, community.Name, community.QrToken);

    public static AddressDto ToDto(this Address address) =>
        new(address.StreetName,
            address.StreetNumber,
            address.Coordinates?.Latitude,
            address.Coordinates?.Longitude);

    public static Address ToDomain(this AddressDto dto)
    {
        var coordinates = dto.Latitude.HasValue && dto.Longitude.HasValue
            ? new Coordinates(dto.Latitude.Value, dto.Longitude.Value)
            : null;
        return new Address(dto.Street, dto.Number, coordinates);
    }

    public static RegistrationNumber? ToRegistrationNumber(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : new RegistrationNumber(value);

    public static TaxId? ToTaxId(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : new TaxId(value);
}
