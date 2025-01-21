using System.Net;


namespace PaymentGateway.Api.Integrations.Models
{
    public class AcquiringBankCreateTransactionResponse  
    {
        public bool? authorized { get; set; }//nullable for bad request case
        public Guid? authorization_code { get; set; }

    }
}