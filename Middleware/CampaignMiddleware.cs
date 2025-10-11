// Middleware/CampaignMiddleware.cs
using Microsoft.AspNetCore.Http;

public class CampaignMiddleware
{
    private readonly RequestDelegate _next;

    public CampaignMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, CampaignService campaignService)
    {
        // Extract host (e.g., save-the-forest.yourdomain.com)
        var host = context.Request.Host.Host;

        // Extract subdomain (everything before the first dot)
        string subdomain = host.Split('.')[0];

        // Ignore 'www' or your main domain if needed
        if (subdomain.ToLower() != "www" && subdomain.ToLower() != "yourdomain")
        {
            // Fetch campaign by subdomain
            var campaign = campaignService.GetCampaignBySubdomain(subdomain);

            if (campaign != null)
            {
                // Store campaign in HttpContext for use in controllers
                context.Items["Campaign"] = campaign;
            }
            else
            {
                // Optional: return 404 if subdomain doesn't exist
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Campaign not found");
                return;
            }
        }

        // Continue pipeline
        await _next(context);
    }
}
