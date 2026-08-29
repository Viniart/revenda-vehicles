using Microsoft.OpenApi.Models;
using Revenda.Vehicles.Api.Security;

namespace Revenda.Vehicles.Api.Extensions;

public static class SwaggerExtensions
{
    public const string SecuritySchemeId = "Bearer";

    public static IServiceCollection AddVehiclesSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Revenda - Veículos e Vendas",
                Version = "v1",
                Description = "Estoque de veículos, compra e efetivação da venda."
            });

            options.OperationFilter<WebhookSecretHeaderOperationFilter>();
            options.OperationFilter<AuthenticationRequirementOperationFilter>();

            options.AddSecurityDefinition(SecuritySchemeId, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Token emitido pelo serviço de identidade."
            });
        });

        return services;
    }
}
