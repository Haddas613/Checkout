using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using NUnit.Framework;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Integrations.Models;
using Moq;
using PaymentGateway.Api.Integrations;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using PaymentGateway.Api.Models.Requests;
using Newtonsoft.Json;
using System.Text;
namespace PaymentGateway.Api.Tests;
[TestFixture]
public class PaymentsControllerTests
{
    private const string paymentBaseUrl = "api";
    private const string paymentActionUrl = "Payments";
    private readonly Random _random = new();

    [Test]
    public async Task GetPaymentDetails_Successfully()
    {
        // Arrange
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999).ToString(),
            Currency = Enums.CurrencyEnum.GBP
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"{paymentBaseUrl}/{paymentActionUrl}/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(paymentResponse, payment);
    }

    [Test]
    public async Task CreatePaymentRequest_ShouldProcessAuthorizedTransaction()
    {
        // Arrange
        var mockProcessor = new Mock<IAcquiringBank>();
        var mockMapper = new Mock<IMapper>();
        var mockRepository = new Mock<PaymentsRepository>();
        var request = new PostPaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = Enums.CurrencyEnum.GBP,
            Amount = 100,
            Cvv = "123"
        };

        var processorRequest = new AcquiringBankCreateTransactionRequest
        {
            card_number = "2222405343248877",
            expiry_date = "04/2025",
            currency = "GBP",
            amount = 100,
            cvv = "123"
        };

        var processorResponse = new AcquiringBankCreateTransactionResponse
        {
            authorized = true,
            authorization_code = new Guid("0bb07405-6d44-4b50-a14f-7ae0beff13ad")
        };

        var expectedResponse = new PostPaymentResponse
        {
            CardNumberLastFour = "8877",
            Currency = Enums.CurrencyEnum.GBP,
            Amount = 100,
            Status = Enums.PaymentStatus.Authorized,
            Id = new Guid("0bb07405-6d44-4b50-a14f-7ae0beff13ad")
        };
        mockMapper.Setup(m => m.Map<AcquiringBankCreateTransactionRequest>(It.IsAny<PostPaymentRequest>()))
                  .Returns(processorRequest);

        mockProcessor.Setup(p => p.CreateTransaction(It.IsAny<AcquiringBankCreateTransactionRequest>()))
                      .ReturnsAsync(processorResponse);

        mockMapper.Setup(m => m.Map<PostPaymentResponse>(processorResponse))
                  .Returns(expectedResponse);

        mockMapper.Setup(m => m.Map<PostPaymentResponse>(It.IsAny<AcquiringBankCreateTransactionResponse>()))
                  .Returns(expectedResponse);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        // Configure WebApplicationFactory with mock services
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                // Replace the real services with mocked ones
                services.AddSingleton(mockRepository.Object);
                services.AddSingleton(mockMapper.Object);
                services.AddSingleton(mockProcessor.Object);
            }))
            .CreateClient();

        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act: Send the POST request to the CreatePaymentRequest endpoint
        var response = await client.PostAsync($"{paymentBaseUrl}/{paymentActionUrl}", jsonContent);
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.IsNotNull(paymentResponse);
        Assert.AreEqual(expectedResponse, paymentResponse);
    }
    [Test]
    public async Task CreatePaymentRequest_ShouldHandleUnauthorizedTransaction()
    {
        // Arrange
        var mockPaymentsRepository = new Mock<PaymentsRepository>(); // Mock the repository
        var mockMapper = new Mock<IMapper>(); // Mock AutoMapper
        var mockProcessor = new Mock<IAcquiringBank>(); // Mock AcquiringBank
        var processorRequest = new AcquiringBankCreateTransactionRequest
        {
            card_number = "2222405343248112",
            expiry_date = "01/2026",
            currency = "USD",
            amount = 60000,
            cvv = "456"
        };
        var request = new PostPaymentRequest
        {
            CardNumber = "2222405343248112", // Unauthorized card number
            ExpiryMonth = 1,
            ExpiryYear = 2026,
            Currency = Enums.CurrencyEnum.USD,
            Amount = 60000,
            Cvv = "456"
        };

        // Set up mock processor response for an unauthorized transaction
        var processorResponse = new AcquiringBankCreateTransactionResponse
        {
            authorized = false, // Unauthorized
            authorization_code = null
        };

        mockMapper.Setup(m => m.Map<AcquiringBankCreateTransactionRequest>(It.IsAny<PostPaymentRequest>()))
                .Returns(processorRequest);

        mockProcessor.Setup(p => p.CreateTransaction(It.IsAny<AcquiringBankCreateTransactionRequest>()))
                      .ReturnsAsync(processorResponse);

        mockMapper.Setup(m => m.Map(It.IsAny<PostPaymentRequest>(), It.IsAny<PostPaymentResponse>()))
    .Callback<PostPaymentRequest, PostPaymentResponse>((source, destination) =>
    {
        destination.ExpiryMonth = source.ExpiryMonth;
        destination.ExpiryYear = source.ExpiryYear;
        destination.Amount = source.Amount;
        destination.Currency = source.Currency;
    });

        // Set up mock processor to return the unauthorized response
        mockProcessor.Setup(p => p.CreateTransaction(It.IsAny<AcquiringBankCreateTransactionRequest>()))
                     .ReturnsAsync(processorResponse);

        var expectedResponse = new PostPaymentResponse
        {
            Status = Enums.PaymentStatus.Declined,
            Id = Guid.Empty
        };

        // Set up mock mapper to return the expected response
        mockMapper.Setup(m => m.Map<PostPaymentResponse>(It.IsAny<AcquiringBankCreateTransactionResponse>()))
                  .Returns(expectedResponse);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        // Configure WebApplicationFactory with mock services
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                // Replace the real services with mocked ones
                services.AddSingleton(mockPaymentsRepository.Object);
                services.AddSingleton(mockMapper.Object);
                services.AddSingleton(mockProcessor.Object);
            }))
            .CreateClient();

        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act: Send the POST request to the CreatePaymentRequest endpoint
        var response = await client.PostAsync($"{paymentBaseUrl}/{paymentActionUrl}", jsonContent);
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        Assert.IsNotNull(paymentResponse);
        Assert.AreEqual(expectedResponse.Status, paymentResponse.Status);
        Assert.AreEqual(expectedResponse.Id, paymentResponse.Id);
    }



    [Test]
    public async Task GetPaymentDetails_PaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"{paymentBaseUrl}/{paymentActionUrl}/{Guid.NewGuid()}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// sample of required parameter
    /// </summary>
    [Test]
    public async Task CreatePaymentRequest_MissingRequiredParameter_CardNumber()
    {
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        // Arrange: Create a request missing the required 'CardNumber' field
        var request = new PostPaymentRequest
        {
            // CardNumber is intentionally left out to trigger validation
            ExpiryMonth = 12,
            ExpiryYear = 2025,
            Cvv = "123",
            Amount = 100,
            Currency = Enums.CurrencyEnum.USD
        };
        var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

        // Act: Send the POST request to the CreatePaymentRequest endpoint
        var response = await client.PostAsync($"{paymentBaseUrl}/{paymentActionUrl}", jsonContent);

        // Assert: Verify the response
        Assert.IsNotNull(response);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.IsNotEmpty(content);

        var problemDetails = JsonConvert.DeserializeObject<ProblemDetailsResponse>(content);
        Assert.IsNotNull(problemDetails);
        Assert.AreEqual("One or more validation errors occurred.", problemDetails.Title);
        Assert.AreEqual(400, problemDetails.Status);
        Assert.IsTrue(problemDetails.Errors.ContainsKey("CardNumber"));
        Assert.AreEqual("Card number is required.", problemDetails.Errors["CardNumber"][0]);
    }
    // Helper class to deserialize the problem details response
    public class ProblemDetailsResponse
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
