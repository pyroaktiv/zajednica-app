using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
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

        account.Profile!.Id.ShouldBe(profileId); // same entity, mutated — not replaced
        account.Profile.FirstName.ShouldBe("Petar");
        account.Profile.Phone.ShouldBe("0649999999");
    }

    [Fact]
    public void Delete_soft_deletes_and_clears_the_profile()
    {
        var account = NewAccount();
        account.UpdateProfile("Pera", "Peric", "0641234567", null, null);

        account.Delete();

        account.Status.ShouldBe(AccountStatus.Deleted);
        account.Profile.ShouldBeNull();   // personal data removed
        account.Username.ShouldBe("pera"); // credentials are kept
    }

    [Fact]
    public void Deleted_account_rejects_further_operations()
    {
        var account = NewAccount();
        account.Delete();

        Should.Throw<EntityValidationException>(() => account.VerifyEmail());
        Should.Throw<EntityValidationException>(() => account.UpdateProfile("Pera", "Peric", "064", null, null));
        Should.Throw<EntityValidationException>(() => account.Delete());
    }
}
