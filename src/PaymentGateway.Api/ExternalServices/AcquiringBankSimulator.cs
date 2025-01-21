using System.Reflection.PortableExecutable;

using Microsoft.Extensions.Options;

using PaymentGateway.Api.Integrations.Models;
using PaymentGateway.Api.Models.Helpers.Web;

namespace PaymentGateway.Api.Integrations
{
    public class AcquiringBankSimulator : IAcquiringBank
    {
        private readonly IWebApiClient apiClient;
        private readonly AcquiringBankSettings configuration;
        public AcquiringBankSimulator(IWebApiClient apiClient, IOptions<AcquiringBankSettings> configuration)
        {
            this.apiClient = apiClient;
            this.configuration = configuration.Value;
        }
        public async Task<AcquiringBankCreateTransactionResponse> CreateTransaction(AcquiringBankCreateTransactionRequest paymentTransactionRequest)
        {
            var res = await this.apiClient.Post<AcquiringBankCreateTransactionResponse>(configuration.BaseUrl,configuration.PaymentProduce, paymentTransactionRequest);
            return res;
        }
    }
}