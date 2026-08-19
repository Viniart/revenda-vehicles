using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.Exceptions;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Domain.Entities;

namespace Revenda.Vehicles.Application.UseCases.Sales;

public sealed record StartPurchaseInput(Guid VehicleId, Guid BuyerId);

/// <summary>
/// Reserva o veículo e abre a venda aguardando pagamento. Reserva e venda são gravadas
/// na mesma transação: ou o veículo sai da vitrine com uma venda associada, ou nada muda.
/// </summary>
public sealed class StartPurchaseUseCase : IStartPurchaseUseCase
{
    private readonly IVehicleRepository _vehicles;
    private readonly ISaleRepository _sales;
    private readonly IPaymentCodeGenerator _paymentCodes;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public StartPurchaseUseCase(
        IVehicleRepository vehicles,
        ISaleRepository sales,
        IPaymentCodeGenerator paymentCodes,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _vehicles = vehicles;
        _sales = sales;
        _paymentCodes = paymentCodes;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<SaleOutput> ExecuteAsync(StartPurchaseInput input, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicles.FindByIdAsync(input.VehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException(input.VehicleId);

        var sale = Sale.Start(vehicle, input.BuyerId, _paymentCodes.Generate(), _clock.UtcNow);

        await _sales.AddAsync(sale, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return SaleOutput.From(sale);
    }
}
