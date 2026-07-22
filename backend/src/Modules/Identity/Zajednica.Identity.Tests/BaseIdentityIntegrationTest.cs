using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Zajednica.Api.Controllers.Identity;
using Zajednica.BuildingBlocks.Tests;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Identity.Tests;

public class BaseIdentityIntegrationTest : BaseWebIntegrationTest<IdentityTestFactory>
{
    protected const string ValidPassword = "password123";

    public BaseIdentityIntegrationTest(IdentityTestFactory factory) : base(factory) { }

    protected static AuthenticationController Controller(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IAuthenticationService>());

    protected static IdentityDbContext Db(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

    protected static string UniqueEmail() => $"{Guid.NewGuid():N}@test.local";

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

    protected static async Task<(string email, Guid accountId)> RegisterVerifiedAsync(IServiceScope scope, string? email = null)
    {
        var (registeredEmail, accountId) = await RegisterAsync(scope, email);

        var db = Db(scope);
        var token = db.EmailVerificationTokens.Single(t => t.AccountId == accountId);
        await Controller(scope).VerifyEmail(new VerifyEmailRequest(token.Token), default);

        return (registeredEmail, accountId);
    }

    protected static async Task<AuthTokens> LoginAsync(IServiceScope scope, string usernameOrEmail)
    {
        var result = await Controller(scope).Login(new LoginRequest(usernameOrEmail, ValidPassword), default);
        return Value<AuthTokens>(result.Result!);
    }

    protected static T Value<T>(IActionResult result) => (T)((ObjectResult)result).Value!;
}
