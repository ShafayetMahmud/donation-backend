public class DonateRequest
{
    public string WalletType { get; set; } = "bkash";
    public decimal Amount { get; set; }
    public string? SenderPhone { get; set; }
}