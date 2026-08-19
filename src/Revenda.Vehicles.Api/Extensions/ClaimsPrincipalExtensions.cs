using System.Security.Claims;

namespace Revenda.Vehicles.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Único dado do comprador que este serviço conhece. Vem do "sub" do token emitido
    /// pelo serviço de identidade.
    /// </summary>
    public static Guid GetBuyerId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(subject, out var buyerId)
            ? buyerId
            : throw new UnauthorizedAccessException("Token sem identificador de comprador.");
    }
}
