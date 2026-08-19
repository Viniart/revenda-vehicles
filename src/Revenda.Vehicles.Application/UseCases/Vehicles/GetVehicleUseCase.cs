using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.Exceptions;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.Ports.Output;

namespace Revenda.Vehicles.Application.UseCases.Vehicles;

public sealed class GetVehicleUseCase : IGetVehicleUseCase
{
    private readonly IVehicleRepository _vehicles;

    public GetVehicleUseCase(IVehicleRepository vehicles) => _vehicles = vehicles;

    public async Task<VehicleOutput> ExecuteAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles.FindByIdAsync(vehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException(vehicleId);

        return VehicleOutput.From(vehicle);
    }
}
