using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Revenda.Vehicles.Api.Contracts;
using Revenda.Vehicles.Api.Extensions;
using Revenda.Vehicles.Api.Security;
using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.UseCases.Sales;

namespace Revenda.Vehicles.Api.Controllers;

[ApiController]
[Route("sales")]
[Produces("application/json")]
[Authorize(Policy = AuthorizationPolicies.BuyerOnly)]
public sealed class SalesController : ControllerBase
{
    private readonly IStartPurchaseUseCase _startPurchase;
    private readonly IListBuyerSalesUseCase _listBuyerSales;

    public SalesController(IStartPurchaseUseCase startPurchase, IListBuyerSalesUseCase listBuyerSales)
    {
        _startPurchase = startPurchase;
        _listBuyerSales = listBuyerSales;
    }

    /// <summary>
    /// Inicia a compra: reserva o veículo e devolve o código que o gateway usará para
    /// confirmar o pagamento.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SaleOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartPurchase(
        [FromBody] StartPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        var output = await _startPurchase.ExecuteAsync(
            new StartPurchaseInput(request.VehicleId, User.GetBuyerId()),
            cancellationToken);

        return CreatedAtAction(nameof(ListMine), null, output);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<SaleOutput>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SaleOutput>>> ListMine(CancellationToken cancellationToken) =>
        Ok(await _listBuyerSales.ExecuteAsync(User.GetBuyerId(), cancellationToken));
}
