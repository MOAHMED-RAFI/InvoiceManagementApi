using InvoiceManagementApi.Dtos;
using InvoiceManagementApi.Models;
using InvoiceManagementApi.Services;

namespace InvoiceManagementApi.Endpoints;

public static class InvoiceEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices").WithTags("Invoices");

        group.MapPost("/", async (CreateInvoiceRequest request, IInvoiceService service, CancellationToken ct) =>
        {
            var errors = Validate(request);
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var result = await service.CreateAsync(request, ct);

            return result.Kind == ResultKind.Ok
                ? Results.Created($"/api/invoices/{result.Value!.Id}", result.Value)
                : result.ToHttpResult();
        });

        group.MapGet("/", async (
            InvoiceStatus? status, int? shipmentId, IInvoiceService service, CancellationToken ct) =>
            Results.Ok(await service.ListAsync(status, shipmentId, ct)));

        group.MapGet("/{id:int}", async (int id, IInvoiceService service, CancellationToken ct) =>
        {
            var invoice = await service.GetAsync(id, ct);
            return invoice is null
                ? Results.NotFound(new { error = $"Invoice {id} was not found." })
                : Results.Ok(invoice);
        });

        group.MapPost("/{id:int}/issue", async (int id, IInvoiceService service, CancellationToken ct) =>
            (await service.IssueAsync(id, ct)).ToHttpResult());

        group.MapPost("/{id:int}/pay", async (int id, IInvoiceService service, CancellationToken ct) =>
            (await service.MarkPaidAsync(id, ct)).ToHttpResult());
    }

    private static Dictionary<string, string[]> Validate(CreateInvoiceRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.ShipmentId <= 0)
            errors["shipmentId"] = new[] { "A valid shipmentId is required." };

        if (request.TaxRate < 0 || request.TaxRate > 100)
            errors["taxRate"] = new[] { "Tax rate must be between 0 and 100." };

        if (!string.IsNullOrWhiteSpace(request.Currency) && request.Currency.Trim().Length != 3)
            errors["currency"] = new[] { "Currency must be a 3-letter code, e.g. INR or USD." };

        if (request.Lines is null || request.Lines.Count == 0)
        {
            errors["lines"] = new[] { "At least one invoice line is required." };
        }
        else
        {
            for (var i = 0; i < request.Lines.Count; i++)
            {
                var line = request.Lines[i];

                if (string.IsNullOrWhiteSpace(line.Description))
                    errors[$"lines[{i}].description"] = new[] { "Description is required." };
                if (line.Quantity <= 0)
                    errors[$"lines[{i}].quantity"] = new[] { "Quantity must be greater than 0." };
                if (line.UnitPrice < 0)
                    errors[$"lines[{i}].unitPrice"] = new[] { "Unit price cannot be negative." };
            }
        }

        return errors;
    }
}
