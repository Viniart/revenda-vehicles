using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Revenda.Vehicles.Api.Security;

namespace Revenda.Vehicles.Api.Extensions;

public static class AuthenticationExtensions
{
    /// <summary>
    /// O token é emitido pelo serviço de identidade e validado aqui pela chave pública
    /// baixada do JWKS dele. Nenhum segredo é compartilhado entre os dois serviços.
    /// </summary>
    public static IServiceCollection AddVehiclesAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var authority = configuration["Jwt:Authority"];
        var audience = configuration["Jwt:Audience"];
        var issuer = configuration["Jwt:Issuer"] ?? authority;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.MapInboundClaims = false;

                // A demonstração roda em HTTP local; em produção o metadata vem por HTTPS.
                options.RequireHttpsMetadata =
                    configuration.GetValue("Jwt:RequireHttpsMetadata", defaultValue: true);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = "role"
                };
            });

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                AuthorizationPolicies.AdminOnly,
                policy => policy.RequireRole(AuthorizationPolicies.AdministratorRole))
            .AddPolicy(
                AuthorizationPolicies.BuyerOnly,
                policy => policy.RequireRole(AuthorizationPolicies.BuyerRole));

        return services;
    }
}
