using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Unit;

public class ValueObjectTests
{
    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void Coordinates_reject_out_of_range_values(decimal latitude, decimal longitude)
        => Should.Throw<EntityValidationException>(() => new Coordinates(latitude, longitude));

    [Fact]
    public void Coordinates_accept_values_within_range()
    {
        var coordinates = new Coordinates(44.8m, 20.4m);
        coordinates.Latitude.ShouldBe(44.8m);
        coordinates.Longitude.ShouldBe(20.4m);
    }

    [Theory]
    [InlineData("1234567")]   // 7 digits
    [InlineData("123456789")] // 9 digits
    [InlineData("1234567a")]  // non-digit
    public void RegistrationNumber_requires_exactly_8_digits(string value)
        => Should.Throw<EntityValidationException>(() => new RegistrationNumber(value));

    [Theory]
    [InlineData("12345678")]   // 8 digits
    [InlineData("1234567890")] // 10 digits
    [InlineData("12345678b")]  // non-digit
    public void TaxId_requires_exactly_9_digits(string value)
        => Should.Throw<EntityValidationException>(() => new TaxId(value));

    [Fact]
    public void Value_objects_compare_by_value()
    {
        new RegistrationNumber("12345678").ShouldBe(new RegistrationNumber("12345678"));
        new TaxId("123456789").ShouldBe(new TaxId("123456789"));
        new Coordinates(44.8m, 20.4m).ShouldBe(new Coordinates(44.8m, 20.4m));
    }
}
