using Revenda.Vehicles.Domain.Entities;
using Revenda.Vehicles.Domain.ValueObjects;

namespace Revenda.Vehicles.UnitTests.TestData;

internal static class VehicleFactory
{
    public static readonly DateTimeOffset Agora = new(2026, 3, 10, 12, 0, 0, TimeSpan.Zero);

    public static Vehicle Disponivel(
        string brand = "Volkswagen",
        string model = "Gol",
        int year = 2022,
        string color = "Prata",
        decimal price = 55_900.00m,
        string plate = "ABC1D23") =>
        Vehicle.Register(
            brand,
            model,
            year,
            color,
            Money.Create(price),
            LicensePlate.Create(plate),
            Agora);

    public static Vehicle Reservado(decimal price = 55_900.00m)
    {
        var vehicle = Disponivel(price: price);
        vehicle.Reserve(Agora);
        return vehicle;
    }

    public static Vehicle Vendido(decimal price = 55_900.00m)
    {
        var vehicle = Reservado(price);
        vehicle.ConfirmSale(Agora);
        return vehicle;
    }
}
