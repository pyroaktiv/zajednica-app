using Zajednica.Api.Middleware;
using Zajednica.Api.Startup;
using Zajednica.BuildingBlocks.Infrastructure.Realtime;
using Zajednica.BuildingBlocks.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.ConfigureOpenApi();
builder.Services.ConfigureCors(builder.Configuration);
builder.Services.ConfigureAuth(builder.Configuration);
builder.Services.RegisterModules(builder.Configuration);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseLocalFiles();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapRealtimeHub();

app.Run();

// Exposes the implicit top-level Program class to the integration-test projects
// (WebApplicationFactory<Program>). Kept in the global namespace so the compiler-generated
// entry point and this declaration are one and the same partial class.
public partial class Program;
