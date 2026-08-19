using System.Text.RegularExpressions;
using Revenda.Vehicles.Domain.Exceptions;

namespace Revenda.Vehicles.Domain.ValueObjects;

/// <summary>
/// Placa brasileira nos dois formatos em circulação: o antigo (ABC1234) e o Mercosul
/// (ABC1D23). É o que identifica o veículo físico e impede o mesmo carro de entrar duas
/// vezes no estoque.
/// </summary>
public sealed partial record LicensePlate
{
    private LicensePlate(string value) => Value = value;

    public string Value { get; }

    public static LicensePlate Create(string? input)
    {
        var normalized = (input ?? string.Empty)
            .Replace("-", string.Empty)
            .Replace(" ", string.Empty)
            .ToUpperInvariant();

        if (!PlatePattern().IsMatch(normalized))
        {
            throw new InvalidVehicleDataException("Placa inválida.");
        }

        return new LicensePlate(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[A-Z]{3}[0-9][0-9A-Z][0-9]{2}$")]
    private static partial Regex PlatePattern();
}
