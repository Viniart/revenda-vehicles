using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Domain.Enums;

namespace Revenda.Vehicles.Application.UseCases.Vehicles;

/// <summary>
/// Filtro exposto ao cliente da API. Não é o mesmo que <see cref="VehicleStatus"/>:
/// "Reserved" é um estado interno do fluxo de pagamento e não interessa a quem consulta.
/// </summary>
public enum VehicleListFilter
{
    ForSale = 1,
    Sold = 2
}

public sealed class ListVehiclesUseCase : IListVehiclesUseCase
{
    private readonly IVehicleRepository _vehicles;

    public ListVehiclesUseCase(IVehicleRepository vehicles) => _vehicles = vehicles;

    public async Task<IReadOnlyList<VehicleOutput>> ExecuteAsync(
        VehicleListFilter filter,
        CancellationToken cancellationToken)
    {
        var status = filter == VehicleListFilter.Sold ? VehicleStatus.Sold : VehicleStatus.Available;
        var vehicles = await _vehicles.ListByStatusAsync(status, cancellationToken);

        return vehicles.Select(VehicleOutput.From).ToList();
    }
}
