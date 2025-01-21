using PaymentGateway.Api.Integrations.Models;

namespace PaymentGateway.Api.Integrations
{
    public interface IAcquiringBank
    {
        Task<AcquiringBankCreateTransactionResponse> CreateTransaction(AcquiringBankCreateTransactionRequest paymentTransactionRequest);
    }
}