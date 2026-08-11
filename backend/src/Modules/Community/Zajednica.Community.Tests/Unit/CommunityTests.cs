using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Community.Core.Domain;

namespace Zajednica.Community.Tests.Unit;

public class CommunityTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Core.Domain.Community NewCommunity()
        => new("Zgrada 1", new Address("Bulevar", "12", new Coordinates(45.25m, 19.83m)), "qr-token", Now);

    [Fact]
    public void UpdateDetails_replaces_the_community_details()
    {
        var community = NewCommunity();

        community.UpdateDetails("  Zgrada 2  ", community.Address, new RegistrationNumber("12345678"),
            new TaxId("123456789"), "160-1234-56");

        community.Name.ShouldBe("Zgrada 2");
        community.RegistrationNumber!.Value.ShouldBe("12345678");
        community.TaxId!.Value.ShouldBe("123456789");
        community.BankAccountNumber.ShouldBe("160-1234-56");
    }

    [Fact]
    public void UpdateDetails_requires_a_name()
    {
        var community = NewCommunity();

        Should.Throw<EntityValidationException>(() =>
            community.UpdateDetails("  ", community.Address, null, null, null));
    }
}
