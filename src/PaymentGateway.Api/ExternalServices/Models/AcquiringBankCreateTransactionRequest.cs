using System.Security;

namespace PaymentGateway.Api.Integrations.Models
{
    public class AcquiringBankCreateTransactionRequest
    {
        public string card_number { get; set; }
        public string expiry_date { get; set; }
        public string currency { get; set; }
        public int amount { get; set; }
        public string cvv { get; set; }
    }
}