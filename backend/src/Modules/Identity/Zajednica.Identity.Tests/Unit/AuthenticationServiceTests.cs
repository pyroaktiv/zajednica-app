using Moq;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Security;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Tests.Unit;

public class AuthenticationServiceTests
{
    private readonly Mock<IAccountRepository> _accounts = new();
    private readonly Mock<IEmailVerificationTokenRepository> _emailTokens = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IAccessTokenGenerator> _accessTokens = new();
    private readonly Mock<ISecureTokenGenerator> _secureTokens = new();
    private readonly Mock<IEmailSender> _email = new();
    private readonly Mock<IAuthTokenSettings> _settings = new();

    public AuthenticationServiceTests()
    {
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("salt.hash");
        _secureTokens.Setup(t => t.Generate()).Returns("verification-token");
        _accounts.Setup(r => r.ExistsByUsername(It.IsAny<string>())).Returns(false);
        _accounts.Setup(r => r.ExistsByEmail(It.IsAny<string>())).Returns(false);
    }

    private AuthenticationService Sut() => new(
        _accounts.Object, _emailTokens.Object, _refreshTokens.Object,
        _passwordHasher.Object, _accessTokens.Object, _secureTokens.Object,
        _email.Object, _settings.Object);

    private static RegisterAccountRequest Registration(string password = "password123") =>
        new("pera", "pera@example.com", password, null, null, null, null);

    [Fact]
    public void Successful_registration_persists_the_account_and_sends_one_activation_email()
    {
        Sut().Register(Registration());

        _accounts.Verify(r => r.Add(It.IsAny<Account>()), Times.Once);
        _email.Verify(e => e.Send("pera@example.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void A_too_short_password_sends_no_email()
    {
        Should.Throw<EntityValidationException>(() => Sut().Register(Registration("short")));

        _email.Verify(e => e.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void A_taken_username_sends_no_email()
    {
        _accounts.Setup(r => r.ExistsByUsername(It.IsAny<string>())).Returns(true);

        Should.Throw<EntityValidationException>(() => Sut().Register(Registration()));

        _email.Verify(e => e.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
