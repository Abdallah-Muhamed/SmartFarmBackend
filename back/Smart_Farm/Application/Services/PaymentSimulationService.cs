using Smart_Farm.DTOS;

namespace Smart_Farm.Application.Services;

/// <summary>
/// Simulates real payment gateways for demo/testing (Stripe-like cards + mobile wallet).
/// </summary>
public class PaymentSimulationService
{
    private static readonly HashSet<string> SuccessCards = new(StringComparer.Ordinal)
    {
        "4242424242424242",
        "4111111111111111",
        "5555555555554444",
    };

    private static readonly HashSet<string> DeclineCards = new(StringComparer.Ordinal)
    {
        "4000000000000002",
        "4000000000009995",
    };

    public PaymentResultDto ProcessCard(CardPaymentRequestDto request)
    {
        var digits = new string((request.CardNumber ?? "").Where(char.IsDigit).ToArray());

        if (digits.Length < 13 || digits.Length > 19)
        {
            return Fail("card", request.Amount, "رقم البطاقة غير صالح.");
        }

        if (string.IsNullOrWhiteSpace(request.CardName))
            return Fail("card", request.Amount, "اسم حامل البطاقة مطلوب.");

        if (string.IsNullOrWhiteSpace(request.Expiry) || !request.Expiry.Contains('/'))
            return Fail("card", request.Amount, "تاريخ الانتهاء غير صالح (MM/YY).");

        if (string.IsNullOrWhiteSpace(request.Cvv) || request.Cvv.Length < 3)
            return Fail("card", request.Amount, "رمز CVV غير صالح.");

        if (DeclineCards.Contains(digits))
            return Fail("card", request.Amount, "تم رفض البطاقة من البنك (محاكاة).");

        if (!SuccessCards.Contains(digits))
            return Fail("card", request.Amount, "بطاقة الاختبار غير معروفة. استخدم 4242 4242 4242 4242.");

        return Success("card", request.Amount, "تم خصم المبلغ بنجاح عبر البطاقة (محاكاة Stripe).");
    }

    public PaymentResultDto ProcessWallet(WalletPaymentRequestDto request)
    {
        var phone = new string((request.Phone ?? "").Where(char.IsDigit).ToArray());

        if (phone.Length != 11 || !phone.StartsWith("01"))
            return Fail("wallet", request.Amount, "رقم المحفظة يجب أن يكون 11 رقمًا ويبدأ بـ 01.");

        if (string.IsNullOrWhiteSpace(request.Otp) || request.Otp.Trim().Length != 6)
            return Fail("wallet", request.Amount, "رمز التحقق OTP يجب أن يكون 6 أرقام.");

        if (request.Otp.Trim() != "123456")
            return Fail("wallet", request.Amount, "رمز OTP غير صحيح. للاختبار استخدم 123456.");

        var provider = string.IsNullOrWhiteSpace(request.Provider) ? "vodafone_cash" : request.Provider;
        return Success("wallet", request.Amount, $"تم الدفع عبر {provider} بنجاح (محاكاة).");
    }

    private static PaymentResultDto Success(string method, decimal amount, string message) => new()
    {
        Success = true,
        TransactionId = $"sim_{method}_{Guid.NewGuid():N}"[..24],
        Status = "succeeded",
        Message = message,
        PaymentMethod = method,
        Amount = amount,
        ProcessedAt = DateTime.UtcNow,
    };

    private static PaymentResultDto Fail(string method, decimal amount, string message) => new()
    {
        Success = false,
        TransactionId = null,
        Status = "failed",
        Message = message,
        PaymentMethod = method,
        Amount = amount,
        ProcessedAt = DateTime.UtcNow,
    };
}
