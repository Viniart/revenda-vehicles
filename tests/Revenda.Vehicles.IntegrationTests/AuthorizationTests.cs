using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace Revenda.Vehicles.IntegrationTests;

public class AuthorizationTests : IClassFixture<VehiclesApiFactory>
{
    private readonly VehiclesApiFactory _factory;

    public AuthorizationTests(VehiclesApiFactory factory) => _factory = factory;

    [Fact]
    public async Task CadastroDeVeiculo_DeveResponder401_SemToken()
    {
        var resposta = await _factory.CreateClient().PostAsJsonAsync("/vehicles", VeiculoValido());

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CadastroDeVeiculo_DeveResponder403_QuandoOTokenEDeComprador()
    {
        var comprador = ClienteComo("Buyer");

        var resposta = await comprador.PostAsJsonAsync("/vehicles", VeiculoValido());

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Compra_DeveResponder403_QuandoOTokenEDeAdministrador()
    {
        var admin = ClienteComo("Administrator");

        var resposta = await admin.PostAsJsonAsync("/sales", new { vehicleId = Guid.NewGuid() });

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Listagem_DeveSerPublica()
    {
        var resposta = await _factory.CreateClient().GetAsync("/vehicles?status=forSale");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Webhook_DeveResponder401_SemOSegredoCombinado()
    {
        var resposta = await _factory.CreateClient().PostAsJsonAsync("/payments/webhook", new
        {
            paymentCode = "PAY-INEXISTENTE",
            status = "approved"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Webhook_DeveResponder404_QuandoOCodigoNaoExiste()
    {
        var gateway = _factory.CreateClient();
        gateway.DefaultRequestHeaders.Add("X-Webhook-Secret", VehiclesApiFactory.WebhookSecret);

        var resposta = await gateway.PostAsJsonAsync("/payments/webhook", new
        {
            paymentCode = "PAY-INEXISTENTE",
            status = "approved"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private HttpClient ClienteComo(string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.CreateToken(Guid.NewGuid(), role));

        return client;
    }

    private static object VeiculoValido() => new
    {
        brand = "Fiat",
        model = "Argo",
        year = 2023,
        color = "Branco",
        price = 72_500m,
        licensePlate = "XYZ9A88"
    };
}
