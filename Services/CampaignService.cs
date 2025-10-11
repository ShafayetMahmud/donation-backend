// Services/CampaignService.cs
using System.Text.Json;

public class CampaignService
{
    // Always points to the folder where the app is running
    private readonly string _dataFolder = Path.Combine(AppContext.BaseDirectory, "campaign-data");

    public CampaignService()
    {
        if (!Directory.Exists(_dataFolder))
            Directory.CreateDirectory(_dataFolder);
    }

    /// <summary>
    /// Create a new campaign and save to JSON file
    /// </summary>
    public Campaign CreateCampaign(Campaign campaign)
    {
        // Assign a unique ID if not provided
        if (string.IsNullOrWhiteSpace(campaign.Id))
            campaign.Id = Guid.NewGuid().ToString();

        // Save file
        string filePath = Path.Combine(_dataFolder, campaign.Id + ".json");
        File.WriteAllText(filePath, JsonSerializer.Serialize(campaign, new JsonSerializerOptions { WriteIndented = true }));

        return campaign;
    }

    /// <summary>
    /// Update an existing campaign
    /// </summary>
    // public Campaign UpdateCampaign(Campaign updatedCampaign)
    // {
    //     if (string.IsNullOrWhiteSpace(updatedCampaign.Id))
    //         throw new ArgumentException("Campaign ID is required for update.");

    //     var filePath = Path.Combine(_dataFolder, updatedCampaign.Id + ".json");

    //     if (!File.Exists(filePath))
    //         throw new FileNotFoundException("Campaign not found.", filePath);

    //     File.WriteAllText(filePath, JsonSerializer.Serialize(updatedCampaign, new JsonSerializerOptions { WriteIndented = true }));

    //     return updatedCampaign;
    // }

    public Campaign UpdateCampaign(Campaign updatedCampaign)
{
    var filePath = Path.Combine(_dataFolder, updatedCampaign.Id + ".json");

    if (!File.Exists(filePath))
        return null;

    // Load existing campaign to preserve Subdomain
    var existingJson = File.ReadAllText(filePath);
    var existingCampaign = JsonSerializer.Deserialize<Campaign>(existingJson);

    // Merge fields (overwrite only what is provided)
    existingCampaign.Name = updatedCampaign.Name;
    existingCampaign.Why = updatedCampaign.Why;
    existingCampaign.WhatFor = updatedCampaign.WhatFor;
    existingCampaign.How = updatedCampaign.How;
    existingCampaign.Contact = updatedCampaign.Contact;
    existingCampaign.Gallery = updatedCampaign.Gallery ?? existingCampaign.Gallery;

    // Write back to file
    File.WriteAllText(filePath, JsonSerializer.Serialize(existingCampaign, new JsonSerializerOptions { WriteIndented = true }));

    return existingCampaign;
}


    /// <summary>
    /// Get a campaign by its subdomain
    /// </summary>
    public Campaign GetCampaignBySubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return null;

        try
        {
            // Ensure directory exists
            if (!Directory.Exists(_dataFolder))
                return null;

            foreach (var file in Directory.GetFiles(_dataFolder, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var campaign = JsonSerializer.Deserialize<Campaign>(json);
                    
                    // Check both exact match and normalized subdomain
                    if (campaign?.Subdomain?.Equals(subdomain, StringComparison.OrdinalIgnoreCase) == true ||
                        campaign?.Name?.ToLower().Replace(" ", "-") == subdomain.ToLower())
                    {
                        return campaign;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading campaign file {file}: {ex.Message}");
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCampaignBySubdomain: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Get all campaigns (optional helper)
    /// </summary>
    public IEnumerable<Campaign> GetAllCampaigns()
    {
        foreach (var file in Directory.GetFiles(_dataFolder, "*.json"))
        {
            var json = File.ReadAllText(file);
            var campaign = JsonSerializer.Deserialize<Campaign>(json);
            if (campaign != null)
                yield return campaign;
        }
    }
}
