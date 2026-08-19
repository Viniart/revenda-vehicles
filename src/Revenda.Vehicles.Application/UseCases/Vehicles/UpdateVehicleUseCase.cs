using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.Exceptions;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Domain.ValueObjects;

namespace Revenda.Vehicles.Application.UseCases.Vehicles;

public sealed record UpdateVehicleInput(
    Guid VehicleId,
    string? Brand,
    string? Model,
    int Year,
    string? Color,
    decimal Price,
    string? LicensePlate);

public sealed class UpdateVehicleUseCase : IUpdateVehicleUseCase
{
    private readonly IVehicleRepository _vehicles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public UpdateVehicleUseCase(IVehicleRepository vehicles, IUnitOfWork unitOfWork, IClock clock)
    {
        _vehicles = vehicles;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<VehicleOutput> ExecuteAsync(UpdateVehicleInput input, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles.FindByIdAsync(input.VehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException(input.VehicleId);

        var licensePlate = LicensePlate.Create(input.LicensePlate);
        var price = Money.Create(input.Price);

        if (await _vehicles.ExistsByLicensePlateAsync(licensePlate, vehicle.Id, cancellationToken))
        {
            throw new DuplicateLicensePlateException(licensePlate.Value);
        }

        vehicle.Update(
            input.Brand,
            input.Model,
            input.Year,
            input.Color,
            price,
            licensePlate,
            _clock.UtcNow);

        await _unitOfWork.CommitAsync(cancellationToken);

        return VehicleOutput.From(vehicle);
    }
}
