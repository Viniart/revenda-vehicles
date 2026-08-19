using Revenda.Vehicles.Domain.Exceptions;

namespace Revenda.Vehicles.Domain.ValueObjects;

/// <summary>Valor monetário em reais, sempre positivo e com duas casas decimais.</summary>
public sealed record Money
{
    public const decimal MaxAmount = 10_000_000m;

    private Money(decimal amount) => Amount = amount;

    public decimal Amount { get; }

    public static Money Create(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidVehicleDataException("O preço deve ser maior que zero.");
        }

        if (amount > MaxAmount)
        {
            throw new InvalidVehicleDataException($"O preço não pode exceder {MaxAmount:N0}.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new InvalidVehicleDataException("O preço deve ter no máximo duas casas decimais.");
        }

        return new Money(amount);
    }

    public override string ToString() => Amount.ToString("F2");
}
