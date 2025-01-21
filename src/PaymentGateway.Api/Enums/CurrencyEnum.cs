using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Enums;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CurrencyEnum : short
{
    [EnumMember(Value = "GBP")]
    GBP = 0,

    [EnumMember(Value = "USD")]
    USD = 1,

    [EnumMember(Value = "EUR")]
    EUR = 2
}