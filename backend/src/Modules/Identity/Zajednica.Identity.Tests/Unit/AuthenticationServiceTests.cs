using Moq;
using Shouldly;
using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Security;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.UseCases;

namespace Zajednica.Identity.Tests.Unit;

// Interaction tests: the register flow is driven with every port mocked, so we assert on the
// side effect that matters — whether the IEmailSender infrastructure port is actually invoked —
// rather than re-checking domain validation rules (those live in the domain/integration tests).
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
        // Only the ports whose output the flow depends on need a return value; the rest are loose.
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("salt.hash");
        _secureTokens.Setup(t => t.Generate()).Returns("verification-token");
        _accounts.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _accounts.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private AuthenticationService Sut() => new(
        _accounts.Object, _emailTokens.Object, _refreshTokens.Object,
        _passwordHasher.Object, _accessTokens.Object, _secureTokens.Object,
        _email.Object, _settings.Object);

    private static RegisterAccountRequest Registration(string password = "password123") =>
        new("pera", "pera@example.com", password, null, null, null, null);

    [Fact]
    public async Task Successful_registration_persists_the_account_and_sends_one_activation_email()
    {
        await Sut().RegisterAsync(Registration());

        _accounts.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
        _email.Verify(e => e.SendAsync("pera@example.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task A_too_short_password_sends_no_email()
    {
        await Should.ThrowAsync<EntityValidationException>(() => Sut().RegisterAsync(Registration("short")));

        _email.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task A_taken_username_sends_no_email()
    {
        _accounts.Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Should.ThrowAsync<EntityValidationException>(() => Sut().RegisterAsync(Registration()));

        _email.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
