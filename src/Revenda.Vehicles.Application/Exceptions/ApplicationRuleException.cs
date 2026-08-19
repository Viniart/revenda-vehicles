namespace Revenda.Vehicles.Application.Exceptions;

public abstract class ApplicationRuleException : Exception
{
    protected ApplicationRuleException(string message) : base(message)
    {
    }
}

public sealed class VehicleNotFoundException : ApplicationRuleException
{
    public VehicleNotFoundException(Guid vehicleId) : base($"Veículo {vehicleId} não encontrado.")
    {
    }
}

public sealed class SaleNotFoundException : ApplicationRuleException
{
    public SaleNotFoundException(string paymentCode)
        : base($"Nenhuma venda encontrada para o código de pagamento {paymentCode}.")
    {
    }
}

public sealed class DuplicateLicensePlateException : ApplicationRuleException
{
    public DuplicateLicensePlateException(string plate)
        : base($"A placa {plate} já pertence a outro veículo do estoque.")
    {
    }
}
