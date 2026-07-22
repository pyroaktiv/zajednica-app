using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class Address : ValueObject
{
    public string StreetName { get; private set; } = null!;
    public string StreetNumber { get; private set; } = null!;
    public Coordinates? Coordinates { get; private set; }

    private Address() { }

    public Address(string streetName, string streetNumber, Coordinates? coordinates = null)
    {
        StreetName = Require(streetName, "Street name");
        StreetNumber = Require(streetNumber, "Street number");
        Coordinates = coordinates;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StreetName;
        yield return StreetNumber;
        if (Coordinates is not null) yield return Coordinates;
    }

    private static string Require(string value, string field)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new EntityValidationException($"{field} is required.");
        return trimmed;
    }
}
