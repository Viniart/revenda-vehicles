namespace Revenda.Vehicles.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}

/// <summary>Dado de entrada que não forma um veículo ou uma venda válida.</summary>
public sealed class InvalidVehicleDataException : DomainException
{
    public InvalidVehicleDataException(string message) : base(message)
    {
    }
}

/// <summary>Operação legítima, mas incompatível com o estado atual do agregado.</summary>
public sealed class InvalidStateTransitionException : DomainException
{
    public InvalidStateTransitionException(string message) : base(message)
    {
    }
}
