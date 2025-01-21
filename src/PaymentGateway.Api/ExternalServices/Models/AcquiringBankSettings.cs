namespace PaymentGateway.Api.Integrations.Models
{
    public class AcquiringBankSettings
    {
        public string BaseUrl { get; set; } // please do not end with slash
        public string PaymentProduce { get; set; }

    }
}