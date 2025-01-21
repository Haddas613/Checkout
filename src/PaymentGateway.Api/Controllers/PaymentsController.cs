using System.Diagnostics.Eventing.Reader;
using System.Security;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Integrations;
using PaymentGateway.Api.Integrations.Models;
using PaymentGateway.Api.Models.Helpers;
using PaymentGateway.Api.Models.Helpers.Web;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

using Swashbuckle.AspNetCore.Annotations;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IAcquiringBank _processor;
    private readonly PaymentsRepository _paymentsRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(PaymentsRepository paymentsRepository, IMapper mapper, IAcquiringBank processor, ILogger<PaymentsController> logger)
    {
        _paymentsRepository = paymentsRepository;
        _mapper = mapper;
        _processor = processor;
        _logger = logger;
    }

    /// <summary>
    /// Get payment details about a specific transaction
    /// </summary>
    /// <param name="id">Guid value that was given when the transaction was created</param>
    /// <returns>PostPaymentResponse with details about the transaction</returns>
    /// <response code="200">Returns the payment details</response>
    /// <response code="404">Cannot find payment with the given Id</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PostPaymentResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        _logger.LogInformation("Retrieving payment details for transaction ID: {TransactionId}", id);

        var payment = _paymentsRepository.Get(id);
        if (payment == null)
        {
            _logger.LogWarning("Payment not found for transaction ID: {TransactionId}", id);
            return new NotFoundObjectResult(payment);
        }

        _logger.LogInformation("Successfully retrieved payment details for transaction ID: {TransactionId}", id);
        return new OkObjectResult(payment);
    }

    /// <summary>
    /// Create a payment request
    /// </summary>
    /// <param name="model">The payment request model containing card details</param>
    /// <returns>Returns a payment response</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Create a payment request", Description = "This API call creates a new payment request for the given details.")]
    public async Task<ActionResult<PostPaymentResponse>> CreatePaymentRequest([FromBody, SwaggerParameter("The payment request model", Required = true)] PostPaymentRequest model)
    {
        _logger.LogInformation("Starting to create a payment request. Model: {Model}", model);

        SecureString secureCardNumber = new SecureString();
        SecureMethods.SetSecuredString(secureCardNumber, model.CardNumber);
        model.CardNumber = string.Empty;

        _logger.LogDebug("Card number secured and cleared in the model.");

        var processorRequest = _mapper.Map<AcquiringBankCreateTransactionRequest>(model);
        processorRequest.card_number = SecureMethods.GetSecuredString(secureCardNumber);

        try
        {
            _logger.LogInformation("Sending request to acquiring bank for transaction.");
            AcquiringBankCreateTransactionResponse processorResponse = await _processor.CreateTransaction(processorRequest);

            if (processorResponse != null)
            {
                _logger.LogInformation("Successfully received response from acquiring bank. Mapping response to payment result.");
                var paymentResult = _mapper.Map<PostPaymentResponse>(processorResponse);
                paymentResult.CardNumberLastFour = SecureMethods.GetSecuredString(secureCardNumber)
                    .Substring(SecureMethods.GetSecuredString(secureCardNumber).Length - 4, 4);

                _mapper.Map(model, paymentResult);

                if (processorResponse.authorized.HasValue && processorResponse.authorized.Value)
                {
                    _logger.LogInformation("Transaction authorized. Saving payment details to repository.");
                    _paymentsRepository.Add(paymentResult);
                }
                else
                {
                    _logger.LogWarning("Transaction not authorized. Details: {ProcessorResponse}", processorResponse);
                }

                return paymentResult;
            }
            else
            {
                _logger.LogError("Failed to create payment. Response from acquiring bank is null.");
                throw new Exception("Failed to create payment, response is null.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating payment request. Model: {Model}", model);
            return StatusCode(StatusCodes.Status404NotFound, $"Failed to create payment request, failed in calling acquiring bank. parameters are: {model.ToString()}, error message: {ex.Message}");
        }
    }
}