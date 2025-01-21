using System;
using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

using PaymentGateway.Api.Enums;

using Swashbuckle.AspNetCore.Annotations;

namespace PaymentGateway.Api.Models.Requests;

/// <summary>
/// Request model for creating a payment.
/// </summary>
public class PostPaymentRequest
{
    /// <summary>
    /// card number
    /// </summary>
    [Required(ErrorMessage = "Card number is required.")]
    [StringLength(19, MinimumLength = 14, ErrorMessage = "Card number must be between 14 and 19 characters long.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Card number must only contain numeric characters.")]
    public string CardNumber { get; set; }

    /// <summary>
    /// expiry month of the card.
    /// </summary>
    [Required(ErrorMessage = "Expiry month is required.")]
    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12.")]
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// expiry year of the card.
    /// </summary>
    [Required(ErrorMessage = "Expiry year is required.")]
    public int ExpiryYear { get; set; }

    
    [ValidExpiryDateInFutureAttribute]
    [JsonIgnore]
    [SwaggerIgnore]
    public object ExpiryValidation => this;

    /// <summary>
    /// currency of the payment
    /// </summary>
    [Required(ErrorMessage = "Currency is required.")]
    [ValidEnumValue(typeof(CurrencyEnum), ErrorMessage = "Invalid currency value.")]
    public CurrencyEnum Currency { get; set; }

    /// <summary>
    /// The amount for the transaction
    /// </summary>
    [Required(ErrorMessage = "Amount is required.")]
    [NumericValue(ErrorMessage = "The amount must be an integer")]
    public int Amount { get; set; }

    /// <summary>
    /// The CVV of the card.
    /// </summary>
    [Required(ErrorMessage = "Cvv is required.")]
    [StringLength(4, MinimumLength = 3, ErrorMessage = "Cvv must be between 3 and 4 characters long.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Cvv must only contain numeric characters.")]
    public string Cvv { get; set; }

    
    public override string ToString()
    {
        return $"Card Number:{CardNumber}, ExpiryDate: {ExpiryMonth}/{ExpiryYear}, Currency: {Currency}, Amount: {Amount}, CVV:{Cvv}";
    }
    
}