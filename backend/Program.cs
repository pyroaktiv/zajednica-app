var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()
    )
);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


var app = builder.Build();

app.UseCors();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
