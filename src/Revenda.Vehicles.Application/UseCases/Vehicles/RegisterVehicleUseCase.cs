using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.Exceptions;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Domain.ValueObjects;

namespace Revenda.Vehicles.Application.UseCases.Vehicles;

public sealed record RegisterVehicleInput(
    string? Brand,
    string? Model,
    int Year,
    string? Color,
    decimal Price,
    string? LicensePlate);

public sealed class RegisterVehicleUseCase : IRegisterVehicleUseCase
{
    private readonly IVehicleRepository _vehicles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RegisterVehicleUseCase(IVehicleRepository vehicles, IUnitOfWork unitOfWork, IClock clock)
    {
        _vehicles = vehicles;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<VehicleOutput> ExecuteAsync(RegisterVehicleInput input, CancellationToken cancellationToken)
    {
        var licensePlate = LicensePlate.Create(input.LicensePlate);
        var price = Money.Create(input.Price);

        if (await _vehicles.ExistsByLicensePlateAsync(licensePlate, ignoredVehicleId: null, cancellationToken))
        {
            throw new DuplicateLicensePlateException(licensePlate.Value);
        }

        var vehicle = Vehicle.Register(
            input.Brand,
            input.Model,
            input.Year,
            input.Color,
            price,
            licensePlate,
            _clock.UtcNow);

        await _vehicles.AddAsync(vehicle, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return VehicleOutput.From(vehicle);
    }
}
