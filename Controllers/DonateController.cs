using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace BkashBackend.Controllers
{
    [ApiController]
[Route("api/[controller]")]
public class DonateController : ControllerBase
{
    private readonly BkashService _bkashService;

    public DonateController(BkashService bkashService)
    {
        _bkashService = bkashService;
    }

    [HttpPost("wallet")]
    public async Task<IActionResult> DonateWallet([FromBody] DonateRequest request)
    {
        var paymentResponse = await _bkashService.CreatePaymentAsync(request);
        if (paymentResponse == null)
            return StatusCode(500, new { message = "Payment creation failed." });

        return Ok(paymentResponse);
    }

    [HttpGet("wallet")]
    public IActionResult WalletGetTest()
    {
        return Ok(new { message = "Use POST to donate" });
    }
}

}
