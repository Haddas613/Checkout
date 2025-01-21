using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Enums;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentStatus
{
    Authorized,
    Declined,//status code ok 
    Rejected// bad request TODO
}