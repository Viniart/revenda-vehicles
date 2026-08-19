using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Testcontainers.PostgreSql;

namespace Revenda.Vehicles.IntegrationTests;

/// <summary>
/// Sobe a API contra um PostgreSQL real e assina tokens com uma chave de teste, no lugar
/// do serviço de identidade. O que se valida aqui é o comportamento desta API diante de um
/// token válido, não a emissão do token.
/// </summary>
public sealed class VehiclesApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Issuer = "https://identity.testes";
    public const string Audience = "revenda-vehicles";
    public const string WebhookSecret = "segredo-de-teste";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("revenda_vehicles")
        .Build();

    private readonly RSA _signingKey = RSA.Create(2048);

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        _signingKey.Dispose();
    }

    public string CreateToken(Guid subject, string role)
    {
        var credentials = new SigningCredentials(
            new RsaSecurityKey(_signingKey) { KeyId = "testes" },
            SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: [new Claim("sub", subject.ToString()), new Claim("role", role)],
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["Jwt:Issuer"] = Issuer,
                ["Jwt:Audience"] = Audience,
                ["Jwt:RequireHttpsMetadata"] = "false",
                ["PaymentWebhook:Secret"] = WebhookSecret
            }));

        builder.ConfigureTestServices(services =>
            services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>>(
                new UseLocalSigningKey(_signingKey)));
    }

    /// <summary>Substitui o download do JWKS pela chave de teste, sem alterar o resto da validação.</summary>
    private sealed class UseLocalSigningKey : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly RSA _key;

        public UseLocalSigningKey(RSA key) => _key = key;

        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            options.Authority = null;
            options.MetadataAddress = null!;
            options.ConfigurationManager = null;
            options.TokenValidationParameters.IssuerSigningKey =
                new RsaSecurityKey(_key) { KeyId = "testes" };
        }
    }
}
