using System.Text.Json.Serialization;

namespace Smart_Farm.DTOS;

public class CardPaymentRequestDto
{
    [JsonPropertyName("cardNumber")]
    public string? CardNumber { get; set; }
    [JsonPropertyName("cardName")]
    public string? CardName { get; set; }
    [JsonPropertyName("expiry")]
    public string? Expiry { get; set; }
    [JsonPropertyName("cvv")]
    public string? Cvv { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}

public class WalletPaymentRequestDto
{
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    [JsonPropertyName("otp")]
    public string? Otp { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("provider")]
    public string? Provider { get; set; }
}

public class PaymentResultDto
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("paymentMethod")]
    public string? PaymentMethod { get; set; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    [JsonPropertyName("processedAt")]
    public DateTime ProcessedAt { get; set; }
}
