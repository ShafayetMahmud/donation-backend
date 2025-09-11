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
        private readonly HttpClient _httpClient;
        public DonateController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        [HttpGet("wallet-test")]
        public async Task<IActionResult> WalletTest()
        {
            // Simulate a POST request
            var fakeRequest = new DonateRequest
            {
                WalletType = "bkash",
                Amount = 100,
                SenderPhone = "017XXXXXXXX"
            };

            var token = await GetAuthTokenAsync();
            if (token == null)
                return Unauthorized(new { message = "Unable to authenticate with bKash." });

            var paymentResponse = await CreatePaymentAsync(token, fakeRequest);
            if (paymentResponse == null)
                return StatusCode(500, new { message = "Payment creation failed." });

            return Ok(paymentResponse);
        }


        // [HttpPost("wallet")]
        // public async Task<IActionResult> DonateWallet([FromBody] DonateRequest request)
        // {
        //     var token = await GetAuthTokenAsync();
        //     if (token == null)
        //     {
        //         return Unauthorized(new { message = "Unable to authenticate with bKash." });
        //     }

        //     var paymentResponse = await CreatePaymentAsync(token, request);
        //     if (paymentResponse == null)
        //     {
        //         return StatusCode(500, new { message = "Payment creation failed." });
        //     }

        //     return Ok(paymentResponse);
        // }

        [HttpPost("wallet")]
public async Task<IActionResult> DonateWallet([FromBody] DonateRequest request)
{
    // For now, we mock the bKash response
    var mockResponse = new
    {
        transactionID = "MOCK" + DateTime.UtcNow.Ticks,
        amount = request.Amount,
        walletType = request.WalletType,
        phoneNumber = request.SenderPhone,
        status = "success",
        message = "Payment processed successfully."
    };

    return Ok(mockResponse);
}


        private async Task<string?> GetAuthTokenAsync()
        {
            var authData = new
            {
                app_key = "your_app_key",
                app_secret = "your_app_secret"
            };

            var content = new StringContent(JsonSerializer.Serialize(authData));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await _httpClient.PostAsync("https://checkout.sandbox.bka.sh/v1.2.0-beta/tokenized/checkout/token/grant", content);
            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(body); // see the error returned by sandbox

            var responseData = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (responseData.TryGetProperty("id_token", out var idToken))
            {
                return idToken.GetString();
            }

            return null;
        }


        private async Task<JsonElement?> CreatePaymentAsync(string token, DonateRequest request)
        {
            var paymentData = new
            {
                amount = request.Amount,
                orderID = "ORD" + DateTime.UtcNow.Ticks,
                intent = "sale",
                callBackURL = "https://yourdomain.com/callback"
            };

            // Create HttpRequestMessage to set Authorization header
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://checkout.sandbox.bka.sh/v1.2.0-beta/tokenized/checkout/create")
            {
                Content = new StringContent(JsonSerializer.Serialize(paymentData))
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(httpRequest);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<JsonElement>();
            }

            return null;
        }


        [HttpGet("wallet")]
        public IActionResult WalletGetTest()
        {
            return Ok(new { message = "Use POST to donate" });
        }
    }

    public class DonateRequest
    {
        public string WalletType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? SenderPhone { get; set; }
    }
}
