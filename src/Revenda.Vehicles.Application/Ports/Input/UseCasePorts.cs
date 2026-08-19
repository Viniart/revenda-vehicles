using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.UseCases.Sales;
using Revenda.Vehicles.Application.UseCases.Vehicles;

namespace Revenda.Vehicles.Application.Ports.Input;

public interface IRegisterVehicleUseCase
{
    Task<VehicleOutput> ExecuteAsync(RegisterVehicleInput input, CancellationToken cancellationToken);
}

public interface IUpdateVehicleUseCase
{
    Task<VehicleOutput> ExecuteAsync(UpdateVehicleInput input, CancellationToken cancellationToken);
}

public interface IGetVehicleUseCase
{
    Task<VehicleOutput> ExecuteAsync(Guid vehicleId, CancellationToken cancellationToken);
}

public interface IListVehiclesUseCase
{
    Task<IReadOnlyList<VehicleOutput>> ExecuteAsync(
        VehicleListFilter filter,
        CancellationToken cancellationToken);
}

public interface IStartPurchaseUseCase
{
    Task<SaleOutput> ExecuteAsync(StartPurchaseInput input, CancellationToken cancellationToken);
}

public interface IConfirmPaymentUseCase
{
    Task<SaleOutput> ExecuteAsync(ConfirmPaymentInput input, CancellationToken cancellationToken);
}

public interface IListBuyerSalesUseCase
{
    Task<IReadOnlyList<SaleOutput>> ExecuteAsync(Guid buyerId, CancellationToken cancellationToken);
}
