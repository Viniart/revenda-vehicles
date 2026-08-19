using Revenda.Vehicles.Api.Extensions;
using Revenda.Vehicles.Api.Middlewares;
using Revenda.Vehicles.Api.Security;
using Revenda.Vehicles.Application;
using Revenda.Vehicles.Infrastructure;
using Revenda.Vehicles.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddVehiclesApplication()
    .AddVehiclesInfrastructure(builder.Configuration);

builder.Services.Configure<WebhookOptions>(builder.Configuration.GetSection(WebhookOptions.SectionName));
builder.Services.AddScoped<WebhookSecretFilter>();

builder.Services.AddVehiclesAuthentication(builder.Configuration);
builder.Services.AddVehiclesSwagger();
builder.Services.AddControllers();
builder.Services
    .AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres") ?? string.Empty, name: "postgres");

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Revenda - Veículos v1"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

if (builder.Configuration.GetValue("Database:MigrateOnStartup", defaultValue: true))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseBootstrapper>().RunAsync(CancellationToken.None);
}

await app.RunAsync();

public partial class Program;
