using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace Revenda.Vehicles.IntegrationTests;

public class PurchaseFlowTests : IClassFixture<VehiclesApiFactory>
{
    private readonly VehiclesApiFactory _factory;

    public PurchaseFlowTests(VehiclesApiFactory factory) => _factory = factory;

    [Fact]
    public async Task FluxoCompleto_DoCadastroDoVeiculoAEfetivacaoDaCompra()
    {
        var admin = ClienteComo("Administrator");
        var veiculo = await CadastrarVeiculoAsync(admin, preco: 61_500m);

        var comprador = ClienteComo("Buyer");
        var compra = await comprador.PostAsJsonAsync("/sales", new { vehicleId = veiculo!.Id });
        compra.StatusCode.Should().Be(HttpStatusCode.Created);

        var venda = await compra.Content.ReadFromJsonAsync<SaleResponse>();
        venda!.Status.Should().Be("PendingPayment");
        venda.Price.Should().Be(61_500m);

        // Enquanto o pagamento não é confirmado, o veículo sai da vitrine.
        var vitrine = await ListarAsync("forSale");
        vitrine.Should().NotContain(v => v.Id == veiculo.Id);

        var efetivacao = await ConfirmarPagamentoAsync(venda.PaymentCode, "approved");
        efetivacao.StatusCode.Should().Be(HttpStatusCode.OK);

        var vendidos = await ListarAsync("sold");
        vendidos.Should().ContainSingle(v => v.Id == veiculo.Id);

        var minhasCompras = await comprador.GetFromJsonAsync<List<SaleResponse>>("/sales/me");
        minhasCompras.Should().ContainSingle(s => s.Id == venda.Id && s.Status == "Paid");
    }

    [Fact]
    public async Task PagamentoRecusado_DeveDevolverOVeiculoParaAVitrine()
    {
        var veiculo = await CadastrarVeiculoAsync(ClienteComo("Administrator"), preco: 33_000m);
        var comprador = ClienteComo("Buyer");

        var compra = await comprador.PostAsJsonAsync("/sales", new { vehicleId = veiculo!.Id });
        var venda = await compra.Content.ReadFromJsonAsync<SaleResponse>();

        await ConfirmarPagamentoAsync(venda!.PaymentCode, "rejected");

        var vitrine = await ListarAsync("forSale");
        vitrine.Should().ContainSingle(v => v.Id == veiculo.Id);
    }

    [Fact]
    public async Task Listagem_DeveVirOrdenadaDoMaisBaratoParaOMaisCaro()
    {
        var admin = ClienteComo("Administrator");
        await CadastrarVeiculoAsync(admin, preco: 120_000m);
        await CadastrarVeiculoAsync(admin, preco: 29_900m);
        await CadastrarVeiculoAsync(admin, preco: 74_300m);

        var vitrine = await ListarAsync("forSale");

        vitrine.Select(v => v.Price).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Compra_DeveResponder409_QuandoOVeiculoJaEstaReservado()
    {
        var veiculo = await CadastrarVeiculoAsync(ClienteComo("Administrator"), preco: 45_000m);

        var primeiro = ClienteComo("Buyer");
        (await primeiro.PostAsJsonAsync("/sales", new { vehicleId = veiculo!.Id }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var segundo = ClienteComo("Buyer");
        var disputa = await segundo.PostAsJsonAsync("/sales", new { vehicleId = veiculo.Id });

        disputa.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private HttpClient ClienteComo(string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.CreateToken(Guid.NewGuid(), role));

        return client;
    }

    private async Task<VehicleResponse?> CadastrarVeiculoAsync(HttpClient admin, decimal preco)
    {
        var resposta = await admin.PostAsJsonAsync("/vehicles", new
        {
            brand = "Volkswagen",
            model = "Gol",
            year = 2022,
            color = "Prata",
            price = preco,
            licensePlate = GerarPlaca()
        });

        resposta.EnsureSuccessStatusCode();

        return await resposta.Content.ReadFromJsonAsync<VehicleResponse>();
    }

    private async Task<HttpResponseMessage> ConfirmarPagamentoAsync(string paymentCode, string status)
    {
        var gateway = _factory.CreateClient();
        gateway.DefaultRequestHeaders.Add("X-Webhook-Secret", VehiclesApiFactory.WebhookSecret);

        return await gateway.PostAsJsonAsync("/payments/webhook", new { paymentCode, status });
    }

    private async Task<List<VehicleResponse>> ListarAsync(string status)
    {
        var publico = _factory.CreateClient();

        return await publico.GetFromJsonAsync<List<VehicleResponse>>($"/vehicles?status={status}") ?? [];
    }

    private static string GerarPlaca()
    {
        const string letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var random = Random.Shared;

        return string.Concat(
            letras[random.Next(letras.Length)],
            letras[random.Next(letras.Length)],
            letras[random.Next(letras.Length)],
            random.Next(0, 10),
            letras[random.Next(letras.Length)],
            random.Next(0, 10),
            random.Next(0, 10));
    }

    private sealed record VehicleResponse(Guid Id, string Brand, decimal Price, string Status);

    private sealed record SaleResponse(Guid Id, Guid VehicleId, decimal Price, string PaymentCode, string Status);
}
