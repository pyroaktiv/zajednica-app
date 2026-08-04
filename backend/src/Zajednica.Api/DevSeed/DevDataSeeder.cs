using Microsoft.EntityFrameworkCore;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Infrastructure.Database;

namespace Zajednica.Api.DevSeed;

public sealed class DevDataSeeder(
    IServiceScopeFactory scopes,
    ILogger<DevDataSeeder> logger) : IHostedService
{
    public const string DefaultPassword = "lozinka123";

    private static readonly SeedAccount[] Cast =
    [
        new("upravnik", "upravnik@zajednica.local", "Mira", "Upravnik"),
        new("izdavac", "izdavac@zajednica.local", "Petar", "Izdavac"),
        new("potvrdjen", "potvrdjen@zajednica.local", "Jovana", "Komsic"),
        new("kandidat", "kandidat@zajednica.local", "Nikola", "Novak"),
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        if (await db.Accounts.AnyAsync(cancellationToken))
        {
            logger.LogInformation("[DevSeed] Baza vec ima naloge; preskacem seed.");
            return;
        }

        var auth = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

        foreach (var person in Cast)
            RegisterActivated(auth, db, person);

        logger.LogInformation(
            "[DevSeed] Napunjeno {Count} aktiviranih naloga (lozinka: {Password}).",
            Cast.Length, DefaultPassword);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void RegisterActivated(IAuthenticationService auth, IdentityDbContext db, SeedAccount person)
    {
        auth.Register(new RegisterAccountRequest(
            person.Username, person.Email, DefaultPassword, person.FirstName, person.LastName, Phone: null, ContactEmail: null));

        var accountId = db.Accounts.Single(a => a.Username == person.Username).Id;
        var token = db.Verifications.Where(v => v.AccountId == accountId).Select(v => v.Token).Single();

        auth.VerifyEmail(new VerifyEmailRequest(token));
    }

    private sealed record SeedAccount(string Username, string Email, string FirstName, string LastName);
}
