using Revenda.Vehicles.Application.Dtos;
using Revenda.Vehicles.Application.Exceptions;
using Revenda.Vehicles.Application.Ports.Input;
using Revenda.Vehicles.Application.Ports.Output;
using Revenda.Vehicles.Domain.Enums;

namespace Revenda.Vehicles.Application.UseCases.Sales;

public sealed record ConfirmPaymentInput(string PaymentCode, bool Approved);

/// <summary>
/// Efetiva a compra a partir do retorno do gateway. É idempotente: o gateway pode
/// reenviar a mesma notificação e o resultado continua o mesmo.
/// </summary>
public sealed class ConfirmPaymentUseCase : IConfirmPaymentUseCase
{
    private readonly ISaleRepository _sales;
    private readonly IVehicleRepository _vehicles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public ConfirmPaymentUseCase(
        ISaleRepository sales,
        IVehicleRepository vehicles,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _sales = sales;
        _vehicles = vehicles;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<SaleOutput> ExecuteAsync(ConfirmPaymentInput input, CancellationToken cancellationToken)
    {
        var paymentCode = input.PaymentCode?.Trim() ?? string.Empty;

        var sale = await _sales.FindByPaymentCodeAsync(paymentCode, cancellationToken)
            ?? throw new SaleNotFoundException(paymentCode);

        if (sale.Status != SaleStatus.PendingPayment)
        {
            return SaleOutput.From(sale);
        }

        var vehicle = await _vehicles.FindByIdAsync(sale.VehicleId, cancellationToken)
            ?? throw new VehicleNotFoundException(sale.VehicleId);

        if (input.Approved)
        {
            sale.ConfirmPayment(vehicle, _clock.UtcNow);
        }
        else
        {
            sale.Cancel(vehicle, _clock.UtcNow);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return SaleOutput.From(sale);
    }
}
