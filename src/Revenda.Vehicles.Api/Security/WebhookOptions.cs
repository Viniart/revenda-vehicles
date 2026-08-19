namespace Revenda.Vehicles.Api.Security;

public sealed class WebhookOptions
{
    public const string SectionName = "PaymentWebhook";

    public const string HeaderName = "X-Webhook-Secret";

    public string Secret { get; set; } = string.Empty;
}
