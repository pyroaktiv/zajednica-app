using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Api.Controllers.Identity;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Identity.Tests;

/// <summary>
/// Base for the Identity integration tests. They drive the real DI graph (the same
/// <see cref="AuthenticationController"/> + <see cref="IAuthenticationService"/> + EF stack the host
/// wires) against the test database, resolving everything from a request scope like the pipeline
/// does per request. Errors surface as thrown domain exceptions here (the exception-to-status
/// middleware only runs over real HTTP), so negative cases assert on the exception type.
///
/// Each test mints unique credentials (a GUID email) so runs never collide on the unique
/// username/email indexes and the suite stays re-runnable without resetting the database.
/// </summary>
public class BaseIdentityIntegrationTest : BaseWebIntegrationTest<IdentityTestFactory>
{
    protected const string ValidPassword = "password123";

    public BaseIdentityIntegrationTest(IdentityTestFactory factory) : base(factory) { }

    protected static AuthenticationController Controller(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IAuthenticationService>());

    protected static IdentityDbContext Db(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

    protected static string UniqueEmail() => $"{Guid.NewGuid():N}@test.local";

    /// <summary>Registers a fresh account and returns its credentials plus id (still unverified).</summary>
    protected static async Task<(string email, Guid accountId)> RegisterAsync(IServiceScope scope, string? email = null)
    {
        email ??= UniqueEmail();
        await Controller(scope).Register(
            new RegisterAccountRequest(email, email, ValidPassword, null, null, null, null), default);

        var db = Db(scope);
        db.ChangeTracker.Clear();
        var account = db.Accounts.Single(a => a.Email == email);
        return (email, account.Id);
    }

    /// <summary>Registers and then follows the activation flow, leaving a login-ready account.</summary>
    protected static async Task<(string email, Guid accountId)> RegisterVerifiedAsync(IServiceScope scope, string? email = null)
    {
        var (registeredEmail, accountId) = await RegisterAsync(scope, email);

        var db = Db(scope);
        var token = db.EmailVerificationTokens.Single(t => t.AccountId == accountId);
        await Controller(scope).VerifyEmail(new VerifyEmailRequest(token.Token), default);

        return (registeredEmail, accountId);
    }

    /// <summary>Logs in with the shared valid password and returns the issued token pair.</summary>
    protected static async Task<AuthTokens> LoginAsync(IServiceScope scope, string usernameOrEmail)
    {
        var result = await Controller(scope).Login(new LoginRequest(usernameOrEmail, ValidPassword), default);
        return Value<AuthTokens>(result.Result!);
    }

    /// <summary>Unwraps the body of an <c>Ok(payload)</c> action result.</summary>
    protected static T Value<T>(IActionResult result) => (T)((ObjectResult)result).Value!;
}
