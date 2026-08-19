using Revenda.Vehicles.Domain.Enums;
using Revenda.Vehicles.Domain.Exceptions;
using Revenda.Vehicles.Domain.ValueObjects;

namespace Revenda.Vehicles.Domain.Entities;

public sealed class Vehicle
{
    public const int MaxTextLength = 60;
    public const int OldestYear = 1900;

    private Vehicle()
    {
    }

    private Vehicle(
        string brand,
        string model,
        int year,
        string color,
        Money price,
        LicensePlate licensePlate,
        DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        Brand = brand;
        Model = model;
        Year = year;
        Color = color;
        Price = price;
        LicensePlate = licensePlate;
        Status = VehicleStatus.Available;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }

    public string Brand { get; private set; } = null!;

    public string Model { get; private set; } = null!;

    public int Year { get; private set; }

    public string Color { get; private set; } = null!;

    public Money Price { get; private set; } = null!;

    public LicensePlate LicensePlate { get; private set; } = null!;

    public VehicleStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Vehicle Register(
        string? brand,
        string? model,
        int year,
        string? color,
        Money price,
        LicensePlate licensePlate,
        DateTimeOffset now) =>
        new(
            RequireText(brand, "marca"),
            RequireText(model, "modelo"),
            RequireYear(year, now),
            RequireText(color, "cor"),
            price,
            licensePlate,
            now);

    /// <summary>
    /// Só faz sentido corrigir o anúncio enquanto ninguém comprou: alterar preço ou modelo
    /// de um veículo reservado mudaria as condições de uma negociação em andamento.
    /// </summary>
    public void Update(
        string? brand,
        string? model,
        int year,
        string? color,
        Money price,
        LicensePlate licensePlate,
        DateTimeOffset now)
    {
        EnsureStatus(VehicleStatus.Available, "Apenas veículos disponíveis podem ser editados.");

        Brand = RequireText(brand, "marca");
        Model = RequireText(model, "modelo");
        Year = RequireYear(year, now);
        Color = RequireText(color, "cor");
        Price = price;
        LicensePlate = licensePlate;
        UpdatedAt = now;
    }

    public void Reserve(DateTimeOffset now)
    {
        EnsureStatus(VehicleStatus.Available, "O veículo não está disponível para compra.");

        Status = VehicleStatus.Reserved;
        UpdatedAt = now;
    }

    public void ConfirmSale(DateTimeOffset now)
    {
        EnsureStatus(VehicleStatus.Reserved, "Só é possível vender um veículo reservado.");

        Status = VehicleStatus.Sold;
        UpdatedAt = now;
    }

    public void ReleaseReservation(DateTimeOffset now)
    {
        EnsureStatus(VehicleStatus.Reserved, "Não há reserva a liberar para este veículo.");

        Status = VehicleStatus.Available;
        UpdatedAt = now;
    }

    private void EnsureStatus(VehicleStatus expected, string message)
    {
        if (Status != expected)
        {
            throw new InvalidStateTransitionException(message);
        }
    }

    private static string RequireText(string? value, string field)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (normalized.Length is 0 || normalized.Length > MaxTextLength)
        {
            throw new InvalidVehicleDataException($"O campo {field} deve ter entre 1 e {MaxTextLength} caracteres.");
        }

        return normalized;
    }

    private static int RequireYear(int year, DateTimeOffset now)
    {
        // Modelos do ano seguinte já são vendidos no segundo semestre, por isso o limite superior.
        var maxYear = now.Year + 1;

        return year is >= OldestYear && year <= maxYear
            ? year
            : throw new InvalidVehicleDataException($"O ano deve estar entre {OldestYear} e {maxYear}.");
    }
}
