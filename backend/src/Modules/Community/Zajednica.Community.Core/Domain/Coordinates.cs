using Zajednica.BuildingBlocks.Core.Domain;
using Zajednica.BuildingBlocks.Core.Exceptions;

namespace Zajednica.Community.Core.Domain;

public class Coordinates : ValueObject
{
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }

    private Coordinates() { }

    public Coordinates(decimal latitude, decimal longitude)
    {
        if (latitude is < -90 or > 90)
            throw new EntityValidationException("Latitude must be between -90 and 90.");
        if (longitude is < -180 or > 180)
            throw new EntityValidationException("Longitude must be between -180 and 180.");

        Latitude = latitude;
        Longitude = longitude;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
