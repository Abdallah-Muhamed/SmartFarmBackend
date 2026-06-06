using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smart_Farm.Application.Services;
using Smart_Farm.DTOS;

namespace Smart_Farm.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController(PaymentSimulationService payments) : ControllerBase
{
    /// <summary>Simulates card payment (Stripe test cards).</summary>
    [Authorize]
    [HttpPost("card")]
    public ActionResult<PaymentResultDto> PayByCard([FromBody] CardPaymentRequestDto dto)
    {
        if (dto.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        var result = payments.ProcessCard(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Simulates mobile wallet payment (Vodafone Cash style).</summary>
    [Authorize]
    [HttpPost("wallet")]
    public ActionResult<PaymentResultDto> PayByWallet([FromBody] WalletPaymentRequestDto dto)
    {
        if (dto.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");

        var result = payments.ProcessWallet(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
