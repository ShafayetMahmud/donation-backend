public class GalleryImage
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string ImageUrl { get; set; }
    public string UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Campaign Campaign { get; set; }
}
