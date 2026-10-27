using InvoiceManagementApi.Models;

namespace InvoiceManagementApi.Dtos;

// ---------- Shipments ----------
public record CreateShipmentRequest(string Customer, string Origin, string Destination);

public record UpdateShipmentStatusRequest(ShipmentStatus Status);

public record ShipmentResponse(
    int Id,
    string ReferenceNo,
    string Customer,
    string Origin,
    string Destination,
    string Status,
    DateTime CreatedAtUtc);

// ---------- Invoices ----------
public record InvoiceLineRequest(string Description, decimal Quantity, decimal UnitPrice);

public record CreateInvoiceRequest(
    int ShipmentId,
    string? Currency,
    decimal TaxRate,
    List<InvoiceLineRequest> Lines);

public record InvoiceLineResponse(
    int Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Amount);

public record InvoiceResponse(
    int Id,
    string InvoiceNo,
    int ShipmentId,
    string ShipmentReference,
    string Currency,
    decimal TaxRate,
    string Status,
    decimal SubTotal,
    decimal TaxAmount,
    decimal Total,
    DateTime CreatedAtUtc,
    List<InvoiceLineResponse> Lines);
