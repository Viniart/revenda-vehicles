namespace Revenda.Vehicles.Api.Security;

public static class AuthorizationPolicies
{
    /// <summary>Cadastro e edição de veículos são operações da revenda, não do comprador.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Comprar é operação de quem tem cadastro de comprador.</summary>
    public const string BuyerOnly = "BuyerOnly";

    public const string AdministratorRole = "Administrator";

    public const string BuyerRole = "Buyer";
}
