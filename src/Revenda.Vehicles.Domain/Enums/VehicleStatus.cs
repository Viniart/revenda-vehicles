namespace Revenda.Vehicles.Domain.Enums;

public enum VehicleStatus
{
    /// <summary>Disponível para compra e visível na listagem de veículos à venda.</summary>
    Available = 1,

    /// <summary>Compra iniciada, aguardando a confirmação do pagamento.</summary>
    Reserved = 2,

    /// <summary>Pagamento confirmado; aparece na listagem de vendidos.</summary>
    Sold = 3
}
