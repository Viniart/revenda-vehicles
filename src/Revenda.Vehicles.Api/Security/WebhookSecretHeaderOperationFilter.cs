using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Revenda.Vehicles.Api.Security;

/// <summary>
/// O segredo do webhook é lido por um filtro, fora do binding do MVC, então não aparece
/// sozinho no contrato. Sem isto quem integra o gateway não descobre pela documentação
/// que o header é obrigatório, e toda chamada feita pelo Swagger volta 401.
/// </summary>
public sealed class WebhookSecretHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var exigeSegredo = context.ApiDescription.ActionDescriptor.FilterDescriptors
            .Any(descriptor => descriptor.Filter is ServiceFilterAttribute filter
                && filter.ServiceType == typeof(WebhookSecretFilter));

        if (!exigeSegredo)
        {
            return;
        }

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = WebhookOptions.HeaderName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Segredo combinado com o gateway de pagamento.",
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}
