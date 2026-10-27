using System.Text.Json.Serialization;
using InvoiceManagementApi.Data;
using InvoiceManagementApi.Endpoints;
using InvoiceManagementApi.Middleware;
using InvoiceManagementApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---- Services (dependency injection container) ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Show enums as text ("Draft") instead of numbers (0) in JSON
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();




// Create the SQLite database and tables on first run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}


// ---- Middleware pipeline (order matters) ----
app.UseMiddleware<RequestTimingMiddleware>();

if (app.Environment.IsDevelopment())
{
    // OpenAPI document at /openapi/v1.json
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

}

// ---- Endpoints ----
app.MapGet("/", () => Results.Ok(new
{
    name = "Invoice Management API",
    docs = "/openapi/v1.json",
    endpoints = new[] { "/api/shipments", "/api/invoices" }
}));

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapShipmentEndpoints();
app.MapInvoiceEndpoints();



app.Run();
