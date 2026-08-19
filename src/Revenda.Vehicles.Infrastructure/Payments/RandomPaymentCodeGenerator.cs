using Revenda.Vehicles.Application.Ports.Output;

namespace Revenda.Vehicles.Infrastructure.Payments;

/// <summary>
/// Código de conciliação devolvido ao comprador e usado pelo gateway no webhook.
/// Não é sequencial de propósito: um código previsível permitiria confirmar a compra alheia.
/// </summary>
internal sealed class RandomPaymentCodeGenerator : IPaymentCodeGenerator
{
    public string Generate() => $"PAY-{Guid.NewGuid():N}"[..24].ToUpperInvariant();
}
