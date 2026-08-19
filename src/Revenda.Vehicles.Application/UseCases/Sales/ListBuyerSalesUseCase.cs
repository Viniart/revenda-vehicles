using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.Ports.Output;

namespace Revenda.Vehicles.Application.UseCases.Sales;

public sealed class ListBuyerSalesUseCase : IListBuyerSalesUseCase
{
    private readonly ISaleRepository _sales;

    public ListBuyerSalesUseCase(ISaleRepository sales) => _sales = sales;

    public async Task<IReadOnlyList<SaleOutput>> ExecuteAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        var sales = await _sales.ListByBuyerAsync(buyerId, cancellationToken);

        return sales.Select(SaleOutput.From).ToList();
    }
}
