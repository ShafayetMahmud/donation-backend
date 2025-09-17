using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class BkashService
{
    private readonly HttpClient _httpClient;
    private readonly string _sandboxTokenUrl = "https://checkout.sandbox.bka.sh/v1.2.0-beta/tokenized/checkout/token/grant";
    private readonly string _sandboxCreateUrl = "https://checkout.sandbox.bka.sh/v1.2.0-beta/tokenized/checkout/create";

    private readonly string _appKey = "your_app_key";
    private readonly string _appSecret = "your_app_secret";

    // Flag to use mock instead of real API
    private readonly bool _useMock;

    public BkashService(HttpClient httpClient, bool useMock = true)
    {
        _httpClient = httpClient;
        _useMock = useMock;
    }

    public async Task<JsonElement?> CreatePaymentAsync(DonateRequest request)
    {
        if (_useMock)
        {
            // Return mock response
            var mockResponse = new
            {
                transactionID = "MOCK" + DateTime.UtcNow.Ticks,
                amount = request.Amount,
                walletType = request.WalletType,
                phoneNumber = request.SenderPhone,
                status = "success",
                message = "Payment processed successfully (mock)."
            };

            // Convert to JsonElement to match method signature
            var json = JsonSerializer.Serialize(mockResponse);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
        else
        {
            // Real bKash flow
            var token = await GetAuthTokenAsync();
            if (token == null) return null;

            var paymentData = new
            {
                amount = request.Amount,
                orderID = "ORD" + DateTime.UtcNow.Ticks,
                intent = "sale",
                callBackURL = "https://yourdomain.com/callback"
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _sandboxCreateUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(paymentData))
            };
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<JsonElement>();
        }
    }

    private async Task<string?> GetAuthTokenAsync()
    {
        if (_useMock) return "MOCK_TOKEN";

        var authData = new { app_key = _appKey, app_secret = _appSecret };
        var content = new StringContent(JsonSerializer.Serialize(authData));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await _httpClient.PostAsync(_sandboxTokenUrl, content);

        // log response for debugging
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine("bKash token response: " + body);

        if (!response.IsSuccessStatusCode) return null;

        var data = JsonSerializer.Deserialize<JsonElement>(body);
        if (data.TryGetProperty("id_token", out var token)) return token.GetString();

        return null;
    }
}
