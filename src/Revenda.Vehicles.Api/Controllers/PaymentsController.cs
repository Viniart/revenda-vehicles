using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Revenda.Vehicles.Api.Contracts;
using Revenda.Vehicles.Api.Security;
using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.UseCases.Sales;

namespace Revenda.Vehicles.Api.Controllers;

[ApiController]
[Route("payments")]
[AllowAnonymous]
[Produces("application/json")]
public sealed class PaymentsController : ControllerBase
{
    private const string ApprovedStatus = "approved";

    private readonly IConfirmPaymentUseCase _confirmPayment;

    public PaymentsController(IConfirmPaymentUseCase confirmPayment) => _confirmPayment = confirmPayment;

    /// <summary>
    /// Efetiva ou cancela a compra a partir do retorno do gateway de pagamento.
    /// Aceita <c>approved</c> ou <c>rejected</c> no campo status.
    /// </summary>
    [HttpPost("webhook")]
    [ServiceFilter(typeof(WebhookSecretFilter))]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleOutput>> Webhook(
        [FromBody] PaymentWebhookRequest request,
        CancellationToken cancellationToken)
    {
        var approved = string.Equals(request.Status, ApprovedStatus, StringComparison.OrdinalIgnoreCase);

        return await _confirmPayment.ExecuteAsync(
            new ConfirmPaymentInput(request.PaymentCode, approved),
            cancellationToken);
    }
}
