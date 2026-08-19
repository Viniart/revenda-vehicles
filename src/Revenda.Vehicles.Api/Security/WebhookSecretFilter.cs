using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Revenda.Vehicles.Api.Security;

/// <summary>
/// O webhook é chamado pelo gateway, que não tem token de usuário. A autenticação é um
/// segredo combinado, comparado em tempo constante para não vazar informação por timing.
/// </summary>
public sealed class WebhookSecretFilter : IAsyncActionFilter
{
    private readonly WebhookOptions _options;

    public WebhookSecretFilter(IOptions<WebhookOptions> options) => _options = options.Value;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var informado = context.HttpContext.Request.Headers[WebhookOptions.HeaderName].ToString();

        if (!SegredoConfere(informado))
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Não autorizado",
                Detail = "Segredo do webhook ausente ou inválido."
            })
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

            return;
        }

        await next();
    }

    private bool SegredoConfere(string informado)
    {
        if (string.IsNullOrEmpty(_options.Secret) || string.IsNullOrEmpty(informado))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(informado),
            Encoding.UTF8.GetBytes(_options.Secret));
    }
}
