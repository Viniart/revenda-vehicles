using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Revenda.Vehicles.Api.Extensions;

/// <summary>
/// Exige o token no contrato apenas nas operações que de fato exigem. Declarar a exigência
/// no documento inteiro faria a especificação afirmar que a vitrine pública e o webhook do
/// gateway precisam de autenticação, quando nenhum dos dois recebe token de usuário.
/// </summary>
public sealed class AuthenticationRequirementOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        var anonima = metadata.OfType<IAllowAnonymous>().Any();
        var protegida = metadata.OfType<IAuthorizeData>().Any();

        if (anonima || !protegida)
        {
            return;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = SwaggerExtensions.SecuritySchemeId
                    }
                }] = []
            }
        ];
    }
}
