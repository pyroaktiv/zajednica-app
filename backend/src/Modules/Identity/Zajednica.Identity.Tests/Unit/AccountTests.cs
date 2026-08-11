using Shouldly;
using Zajednica.Identity.Core.Domain;

namespace Zajednica.Identity.Tests.Unit;

public class AccountTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Account NewAccount() => new("pera", "pera@example.com", "hash", Now);

    [Fact]
    public void UpdateProfile_mutates_the_existing_profile_in_place()
    {
        var account = NewAccount();
        account.UpdateProfile("Pera", "Peric", "0641234567", null, null);
        var profileId = account.Profile!.Id;

        account.UpdateProfile("Petar", "Peric", "0649999999", null, null);

        account.Profile!.Id.ShouldBe(profileId);
        account.Profile.FirstName.ShouldBe("Petar");
        account.Profile.Phone.ShouldBe("0649999999");
    }
}
