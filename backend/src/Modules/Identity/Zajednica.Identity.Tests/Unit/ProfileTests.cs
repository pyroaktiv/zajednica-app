using Shouldly;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Tests.Unit;

public class ProfileTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Profile ProfileWith(string? firstName, string? lastName, string? phone)
    {
        var account = new Account("pera", "pera@example.com", "hash", Now);
        account.UpdateProfile(firstName, lastName, phone, null, null);
        return account.Profile!;
    }

    [Fact]
    public void HasManagerData_true_when_first_last_and_phone_present() =>
        ProfileWith("Pera", "Peric", "0641234567").HasManagerData().ShouldBeTrue();

    [Theory]
    [InlineData(null, "Peric", "064")]
    [InlineData("Pera", null, "064")]
    [InlineData("Pera", "Peric", null)]
    public void HasManagerData_false_when_any_required_field_missing(string? firstName, string? lastName, string? phone) =>
        ProfileWith(firstName, lastName, phone).HasManagerData().ShouldBeFalse();

    [Fact]
    public void Blank_values_are_stored_as_null()
    {
        var profile = ProfileWith("   ", "Peric", "064");

        profile.FirstName.ShouldBeNull();
        profile.HasManagerData().ShouldBeFalse();
    }

    [Fact]
    public void Values_are_trimmed()
    {
        var profile = ProfileWith("  Pera  ", "  Peric  ", "  064  ");

        profile.FirstName.ShouldBe("Pera");
        profile.LastName.ShouldBe("Peric");
        profile.Phone.ShouldBe("064");
    }
}
