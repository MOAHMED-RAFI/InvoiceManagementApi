using InvoiceManagementApi.Dtos;
using InvoiceManagementApi.Models;
using InvoiceManagementApi.Services;

namespace InvoiceManagementApi.Endpoints;

public static class ShipmentEndpoints
{
    public static void MapShipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/shipments").WithTags("Shipments");

        group.MapPost("/", async (CreateShipmentRequest request, IShipmentService service, CancellationToken ct) =>
        {
            var errors = Validate(request);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var created = await service.CreateAsync(request, ct);
            return Results.Created($"/api/shipments/{created.Id}", created);
        });

        group.MapGet("/", async (ShipmentStatus? status, IShipmentService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(status, ct)));

        group.MapGet("/{id:int}", async (int id, IShipmentService service, CancellationToken ct) =>
        {
            var shipment = await service.GetAsync(id, ct);
            return shipment is null
                ? Results.NotFound(new { error = $"Shipment {id} was not found." })
                : Results.Ok(shipment);
        });

        group.MapPut("/{id:int}/status", async (
            int id, UpdateShipmentStatusRequest request, IShipmentService service, CancellationToken ct) =>
        {
            var result = await service.UpdateStatusAsync(id, request.Status, ct);
            return result.ToHttpResult();
        });
    }

    private static Dictionary<string, string[]> Validate(CreateShipmentRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Customer))
            errors["customer"] = new[] { "Customer is required." };
        if (string.IsNullOrWhiteSpace(request.Origin))
            errors["origin"] = new[] { "Origin is required." };
        if (string.IsNullOrWhiteSpace(request.Destination))
            errors["destination"] = new[] { "Destination is required." };

        return errors;
    }
}
