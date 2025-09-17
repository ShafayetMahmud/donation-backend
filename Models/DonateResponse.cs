public class DonateResponse
{
    public string TransactionID { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string WalletType { get; set; } = "bkash";
    public string? PhoneNumber { get; set; }
    public string Status { get; set; } = "success";
    public string Message { get; set; } = "Payment processed successfully";
}