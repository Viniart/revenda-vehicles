using Revenda.Vehicles.Domain.Enums;
using Revenda.Vehicles.Domain.Exceptions;
using Revenda.Vehicles.Domain.ValueObjects;

namespace Revenda.Vehicles.Domain.Entities;

public sealed class Sale
{
    private Sale()
    {
    }

    private Sale(Guid vehicleId, Guid buyerId, Money price, string paymentCode, DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        VehicleId = vehicleId;
        BuyerId = buyerId;
        Price = price;
        PaymentCode = paymentCode;
        Status = SaleStatus.PendingPayment;
        CreatedAt = now;
    }

    public Guid Id { get; private set; }

    public Guid VehicleId { get; private set; }

    /// <summary>Identificador do comprador vindo do token. Nenhum dado pessoal é guardado aqui.</summary>
    public Guid BuyerId { get; private set; }

    /// <summary>Preço congelado no momento da compra, independente de reajustes posteriores.</summary>
    public Money Price { get; private set; } = null!;

    public string PaymentCode { get; private set; } = null!;

    public SaleStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public static Sale Start(Vehicle vehicle, Guid buyerId, string paymentCode, DateTimeOffset now)
    {
        if (buyerId == Guid.Empty)
        {
            throw new InvalidVehicleDataException("A compra exige um comprador identificado.");
        }

        if (string.IsNullOrWhiteSpace(paymentCode))
        {
            throw new InvalidVehicleDataException("A compra exige um código de pagamento.");
        }

        vehicle.Reserve(now);

        return new Sale(vehicle.Id, buyerId, vehicle.Price, paymentCode, now);
    }

    public void ConfirmPayment(Vehicle vehicle, DateTimeOffset now)
    {
        EnsurePending();
        EnsureSameVehicle(vehicle);

        vehicle.ConfirmSale(now);

        Status = SaleStatus.Paid;
        CompletedAt = now;
    }

    public void Cancel(Vehicle vehicle, DateTimeOffset now)
    {
        EnsurePending();
        EnsureSameVehicle(vehicle);

        vehicle.ReleaseReservation(now);

        Status = SaleStatus.Cancelled;
        CompletedAt = now;
    }

    private void EnsurePending()
    {
        if (Status != SaleStatus.PendingPayment)
        {
            throw new InvalidStateTransitionException("A venda já foi finalizada e não aceita novas mudanças.");
        }
    }

    private void EnsureSameVehicle(Vehicle vehicle)
    {
        if (vehicle.Id != VehicleId)
        {
            throw new InvalidStateTransitionException("O veículo informado não pertence a esta venda.");
        }
    }
}
