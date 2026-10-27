using InvoiceManagementApi.Models;

namespace InvoiceManagementApi.Dtos;

public static class Mappings
{
    public static ShipmentResponse ToResponse(this Shipment s) =>
        new(s.Id, s.ReferenceNo, s.Customer, s.Origin, s.Destination,
            s.Status.ToString(), s.CreatedAtUtc);

    public static InvoiceResponse ToResponse(this Invoice i) =>
        new(
            i.Id,
            i.InvoiceNo,
            i.ShipmentId,
            i.Shipment?.ReferenceNo ?? string.Empty,
            i.Currency,
            i.TaxRate,
            i.Status.ToString(),
            i.SubTotal,
            i.TaxAmount,
            i.Total,
            i.CreatedAtUtc,
            i.Lines.Select(l => new InvoiceLineResponse(
                l.Id, l.Description, l.Quantity, l.UnitPrice, l.Amount)).ToList());
}
