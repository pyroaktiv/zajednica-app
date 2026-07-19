using Moq;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Tests.Unit;

// Interaction tests: the profile flow is driven with the ports mocked, so we assert on the side
// effects that matter — a single commit on update, and no commit on a lookup miss — rather than
// re-checking the domain cleaning rules (those live with the Profile/Account domain tests).
public class ProfileServiceTests
{
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<IIdentityUnitOfWork> _uow = new();

    private ProfileService Sut() => new(_accounts.Object, _uow.Object);

    private static UpdateProfileRequest Update() => new("Pera", "Peric", "060123456", "pera@example.com", null);

    [Fact]
    public async Task Updating_an_existing_profile_commits_once()
    {
        var account = new Account("pera", "pera@example.com", "salt.hash", DateTime.UtcNow);
        _accounts.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var result = await Sut().UpdateAsync(account.Id, Update());

        result.FirstName.ShouldBe("Pera");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Updating_an_unknown_account_throws_and_never_commits()
    {
        _accounts.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Account?)null);

        await Should.ThrowAsync<NotFoundException>(() => Sut().UpdateAsync(Guid.NewGuid(), Update()));

        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
