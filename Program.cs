using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add controllers and Swagger
builder.Services.AddSingleton<CampaignService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: allow everything
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // Production: allow same-origin requests (since frontend and API are on same domain)
            policy.SetIsOriginAllowed(origin => true)  // Allow any origin since we're handling subdomains
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin)) return false;

            try
            {
                var uri = new Uri(origin);
                var domain = uri.Host.ToLower();
                return domain.EndsWith("mudhammataan.com") ||
       domain == "localhost" ||
       domain.EndsWith("azurewebsites.net") ||
       domain == "mudhammataan-app-bcdwa5debqc4h7dj.northeurope-01.azurewebsites.net";
            }
            catch
            {
                return false;
            }
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// "https://your-frontend-domain.com", "https://mudhammataan.com"

var app = builder.Build();

// Enable Swagger (conditionally only if needed)

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only force HTTPS in production and if not already using HTTPS
// Disable HTTPS redirection for now
// app.UseHttpsRedirection();
app.UseStaticFiles();    // Serve Angular files from wwwroot
app.UseRouting();
app.UseCors("AllowFrontend");

// Add subdomain middleware
app.Use(async (context, next) =>
{
    var host = context.Request.Host.Host;
    var path = context.Request.Path.Value;

    // Only process API requests
    if (!path.StartsWith("/api"))
    {
        await next();
        return;
    }

    // Extract subdomain (everything before the first dot)
    var subdomain = host.Split('.')[0];
    if (!string.IsNullOrEmpty(subdomain) && subdomain != "www" && !subdomain.Equals("mudhammataan", StringComparison.OrdinalIgnoreCase))
    {
        // Store subdomain in HttpContext items for later use
        context.Items["subdomain"] = subdomain;
    }

    await next();
});
app.MapControllers();
// app.UseMiddleware<CampaignMiddleware>();


// Fallback to Angular index.html for client-side routing
app.MapFallbackToFile("index.html");

app.Run();
