using Revenda.Vehicles.Domain.Entities;

namespace Revenda.Vehicles.Application.Dtos;

public sealed record VehicleOutput(
    Guid Id,
    string Brand,
    string Model,
    int Year,
    string Color,
    decimal Price,
    string LicensePlate,
    string Status)
{
    public static VehicleOutput From(Vehicle vehicle) =>
        new(
            vehicle.Id,
            vehicle.Brand,
            vehicle.Model,
            vehicle.Year,
            vehicle.Color,
            vehicle.Price.Amount,
            vehicle.LicensePlate.Value,
            vehicle.Status.ToString());
}
