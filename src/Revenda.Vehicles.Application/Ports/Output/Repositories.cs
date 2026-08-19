using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Domain.Enums;
using Revenda.Vehicles.Domain.ValueObjects;

namespace Revenda.Vehicles.Application.Ports.Output;

public interface IVehicleRepository
{
    Task<Vehicle?> FindByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Listagem ordenada por preço, do mais barato para o mais caro.</summary>
    Task<IReadOnlyList<Vehicle>> ListByStatusAsync(VehicleStatus status, CancellationToken cancellationToken);

    Task<bool> ExistsByLicensePlateAsync(
        LicensePlate licensePlate,
        Guid? ignoredVehicleId,
        CancellationToken cancellationToken);

    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken);
}

public interface ISaleRepository
{
    Task<Sale?> FindByPaymentCodeAsync(string paymentCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<Sale>> ListByBuyerAsync(Guid buyerId, CancellationToken cancellationToken);

    Task AddAsync(Sale sale, CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken);
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface IPaymentCodeGenerator
{
    string Generate();
}
