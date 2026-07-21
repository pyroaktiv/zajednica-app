using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Zajednica.BuildingBlocks.Tests;

// Spins up the real host (Program) against a dedicated test database, swapping in the
// module's DbContext. Derive one factory per module (see <Module>TestFactory).
public abstract class BaseTestFactory<TDbContext> : WebApplicationFactory<Program> where TDbContext : DbContext
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            using var scope = BuildServiceProvider(services).CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<TDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<BaseTestFactory<TDbContext>>>();

            CreateTables(db);
            // A module under test also reads through the Api/Internal seams of the modules it calls,
            // so their schemas have to exist in the same test database.
            foreach (var contextType in CollaboratingDbContexts)
                CreateTables((DbContext)scopedServices.GetRequiredService(contextType));

            var path = Path.Combine(".", "..", "..", "..", "TestData");
            SeedTestData(db, path, logger);
        });
    }

    protected virtual IEnumerable<Type> CollaboratingDbContexts => [];

    private static void CreateTables(DbContext context)
    {
        try
        {
            context.Database.EnsureCreated();
            var databaseCreator = context.Database.GetService<IRelationalDatabaseCreator>();
            databaseCreator.CreateTables();
        }
        catch (Exception)
        {
            // CreateTables throws if the schema already exists. Expected when several
            // module DbContexts share one physical test database.
        }
    }

    private static void SeedTestData(DbContext context, string scriptFolder, ILogger logger)
    {
        try
        {
            if (!Directory.Exists(scriptFolder)) return;

            var scriptFiles = Directory.GetFiles(scriptFolder);
            Array.Sort(scriptFiles);
            var script = string.Join('\n', scriptFiles.Select(File.ReadAllText));
            if (!string.IsNullOrWhiteSpace(script))
                context.Database.ExecuteSqlRaw(script);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred seeding the database with test data. Error: {Message}", ex.Message);
        }
    }

    private ServiceProvider BuildServiceProvider(IServiceCollection services) =>
        ReplaceNeededDbContexts(services).BuildServiceProvider();

    // Each module factory removes the real DbContext registration and re-adds it
    // against the test database (see SetupTestContext).
    protected abstract IServiceCollection ReplaceNeededDbContexts(IServiceCollection services);

    protected static Action<DbContextOptionsBuilder> SetupTestContext()
    {
        var host = Environment.GetEnvironmentVariable("TEST_DATABASE_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("TEST_DATABASE_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("TEST_DATABASE_NAME") ?? "zajednica-test";
        var user = Environment.GetEnvironmentVariable("TEST_DATABASE_USERNAME") ?? "zajednica";
        var password = Environment.GetEnvironmentVariable("TEST_DATABASE_PASSWORD") ?? "devpassword";

        var connectionString =
            $"Host={host};Port={port};Database={database};Username={user};Password={password};Include Error Detail=True";

        return opt => opt.UseNpgsql(connectionString);
    }
}
