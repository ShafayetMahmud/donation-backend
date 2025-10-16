// Controllers/CampaignController.cs
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/campaign")]  // Explicitly set the route to lowercase
public class CampaignController : ControllerBase
{
    private readonly CampaignService _service;
    private readonly ILogger<CampaignController> _logger;

    public CampaignController(CampaignService service, ILogger<CampaignController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("create")]
    public IActionResult Create([FromBody] Campaign campaign)
    {
        Console.WriteLine($"Received campaign payload: {campaign?.Name}, {campaign?.Why}, {campaign?.WhatFor}, {campaign?.How}, {campaign?.Contact}, {campaign?.Goals}, {campaign?.Method}, {campaign?.Quote}, {campaign?.Missionquote}, {campaign?.Descriptionone}, {campaign?.Descriptiontwo}, {campaign?.Descriptionthree}, {campaign?.Descriptionfour}");

        if (campaign == null || string.IsNullOrWhiteSpace(campaign.Name))
            return BadRequest("Campaign Name required");

        campaign.Subdomain = campaign.Name.ToLower().Replace(" ", "-");
        var result = _service.CreateCampaign(campaign);

        Console.WriteLine($"Created campaign file: {result.Id}.json");

        return Ok(result);
    }

    [HttpPut("update")]
    public IActionResult Update([FromBody] Campaign updatedCampaign)
    {
        if (string.IsNullOrWhiteSpace(updatedCampaign.Id))
            return BadRequest("Campaign ID required");

        var existing = _service.UpdateCampaign(updatedCampaign);
        if (existing == null)
            return NotFound("Campaign not found");

        return Ok(existing);
    }

    [HttpGet("by-subdomain/{subdomain}")]
    public IActionResult GetBySubdomain(string subdomain)
    {
        _logger.LogInformation($"Searching for campaign with subdomain: {subdomain}");
        
        try
        {
            var campaign = _service.GetCampaignBySubdomain(subdomain);
            if (campaign == null)
            {
                _logger.LogWarning($"Campaign not found for subdomain: {subdomain}");
                return NotFound(new { message = $"No campaign found for subdomain: {subdomain}" });
            }
            
            _logger.LogInformation($"Found campaign: {campaign.Name} for subdomain: {subdomain}");
            return Ok(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting campaign for subdomain: {subdomain}");
            return StatusCode(500, new { message = "Error retrieving campaign" });
        }
    }
}
