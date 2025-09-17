using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(); // Minimal API OpenAPI support

// Enable CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Add HttpClient for DI
builder.Services.AddHttpClient();

// --- REGISTER YOUR CUSTOM SERVICE ---
// Use mock for now
builder.Services.AddScoped(_ => new BkashService(new HttpClient(), useMock: true));


var app = builder.Build();

// OpenAPI (JSON spec) in Development

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowAngular");
app.MapControllers();

app.Run();
