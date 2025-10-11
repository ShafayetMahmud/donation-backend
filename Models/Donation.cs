public class Donation
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string DonorName { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }

    public Campaign Campaign { get; set; }
}
