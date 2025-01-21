using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models.Responses;

public class PostPaymentResponse
{
    public Guid? Id { get; set; }
    public PaymentStatus Status { get; set; }
    public string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public CurrencyEnum Currency { get; set; }
    public int Amount { get; set; }
    public override bool Equals(object obj)
    {
        if (obj is PostPaymentResponse other)
        {
            return CardNumberLastFour == other.CardNumberLastFour
                && Amount == other.Amount
                && Currency == other.Currency
                && ExpiryYear ==  other.ExpiryYear
                && ExpiryMonth == other.ExpiryMonth
                && Id ==  other.Id
                && Status ==  other.Status;
        }
        return false;
    }
}
