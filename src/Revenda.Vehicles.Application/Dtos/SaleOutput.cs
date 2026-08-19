using Revenda.Vehicles.Domain.Entities;

namespace Revenda.Vehicles.Application.Dtos;

public sealed record SaleOutput(
    Guid Id,
    Guid VehicleId,
    Guid BuyerId,
    decimal Price,
    string PaymentCode,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt)
{
    public static SaleOutput From(Sale sale) =>
        new(
            sale.Id,
            sale.VehicleId,
            sale.BuyerId,
            sale.Price.Amount,
            sale.PaymentCode,
            sale.Status.ToString(),
            sale.CreatedAt,
            sale.CompletedAt);
}
