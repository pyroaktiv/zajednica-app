using Moq;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Tests.Unit;

public class ProfileServiceTests
{
    private readonly Mock<IAccountRepository> _accounts = new();

    private ProfileService Sut() => new(_accounts.Object);

    private static UpdateProfileRequestDto Update() => new("Pera", "Peric", "060123456", "pera@example.com");

    [Fact]
    public void Updating_an_existing_profile_persists_once()
    {
        var account = new Account("pera", "pera@example.com", "salt.hash", DateTime.UtcNow);
        _accounts.Setup(r => r.GetById(account.Id)).Returns(account);

        var result = Sut().Update(account.Id, Update());

        result.FirstName.ShouldBe("Pera");
        _accounts.Verify(r => r.Update(account), Times.Once);
    }

    [Fact]
    public void Updating_the_text_fields_keeps_the_existing_image()
    {
        var account = new Account("pera", "pera@example.com", "salt.hash", DateTime.UtcNow);
        account.SetProfileImage("images/pera.jpg");
        _accounts.Setup(r => r.GetById(account.Id)).Returns(account);

        var result = Sut().Update(account.Id, Update());

        result.ImageUrl.ShouldBe($"api/profiles/{account.Id}/image");
    }

    [Fact]
    public void Setting_and_removing_the_image_leaves_the_text_untouched()
    {
        var account = new Account("pera", "pera@example.com", "salt.hash", DateTime.UtcNow);
        _accounts.Setup(r => r.GetById(account.Id)).Returns(account);
        Sut().Update(account.Id, Update());

        var afterSet = Sut().SetImage(account.Id, new SetProfileImageRequestDto("images/pera.jpg"));
        afterSet.FirstName.ShouldBe("Pera");
        afterSet.ImageUrl.ShouldBe($"api/profiles/{account.Id}/image");

        Sut().RemoveImage(account.Id);
        var afterRemove = Sut().Get(account.Id);
        afterRemove.FirstName.ShouldBe("Pera");
        afterRemove.ImageUrl.ShouldBeNull();
    }

    [Fact]
    public void Updating_an_unknown_account_throws_and_never_persists()
    {
        _accounts.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((Account?)null);

        Should.Throw<NotFoundException>(() => Sut().Update(Guid.NewGuid(), Update()));

        _accounts.Verify(r => r.Update(It.IsAny<Account>()), Times.Never);
    }
}
