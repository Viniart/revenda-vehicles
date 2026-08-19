using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Revenda.Vehicles.Api.Contracts;
using Revenda.Vehicles.Api.Security;
using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.UseCases.Vehicles;

namespace Revenda.Vehicles.Api.Controllers;

[ApiController]
[Route("vehicles")]
[Produces("application/json")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IRegisterVehicleUseCase _registerVehicle;
    private readonly IUpdateVehicleUseCase _updateVehicle;
    private readonly IGetVehicleUseCase _getVehicle;
    private readonly IListVehiclesUseCase _listVehicles;

    public VehiclesController(
        IRegisterVehicleUseCase registerVehicle,
        IUpdateVehicleUseCase updateVehicle,
        IGetVehicleUseCase getVehicle,
        IListVehiclesUseCase listVehicles)
    {
        _registerVehicle = registerVehicle;
        _updateVehicle = updateVehicle;
        _getVehicle = getVehicle;
        _listVehicles = listVehicles;
    }

    /// <summary>
    /// Lista o estoque ordenado por preço, do mais barato para o mais caro.
    /// Use <c>forSale</c> para a vitrine e <c>sold</c> para o histórico de vendas.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleOutput>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VehicleOutput>>> List(
        [FromQuery] VehicleListFilter status = VehicleListFilter.ForSale,
        CancellationToken cancellationToken = default) =>
        Ok(await _listVehicles.ExecuteAsync(status, cancellationToken));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VehicleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleOutput>> GetById(Guid id, CancellationToken cancellationToken) =>
        await _getVehicle.ExecuteAsync(id, cancellationToken);

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(VehicleOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] SaveVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var output = await _registerVehicle.ExecuteAsync(
            new RegisterVehicleInput(
                request.Brand,
                request.Model,
                request.Year,
                request.Color,
                request.Price,
                request.LicensePlate),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = output.Id }, output);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(typeof(VehicleOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleOutput>> Update(
        Guid id,
        [FromBody] SaveVehicleRequest request,
        CancellationToken cancellationToken) =>
        await _updateVehicle.ExecuteAsync(
            new UpdateVehicleInput(
                id,
                request.Brand,
                request.Model,
                request.Year,
                request.Color,
                request.Price,
                request.LicensePlate),
            cancellationToken);
}
