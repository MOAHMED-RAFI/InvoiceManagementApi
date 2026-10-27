namespace InvoiceManagementApi.Models;

public enum ShipmentStatus
{
    Booked,
    InTransit,
    Delivered
}

public enum InvoiceStatus
{
    Draft,
    Issued,
    Paid
}

public class Shipment
{
    public int Id { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Booked;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Invoice> Invoices { get; set; } = new();
}

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;

    public int ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    public string Currency { get; set; } = "INR";
    public decimal TaxRate { get; set; }              // percentage, e.g. 18 means 18%
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<InvoiceLine> Lines { get; set; } = new();

    // Calculated values (not stored in the database)
    public decimal SubTotal => Lines.Sum(l => l.Amount);

    public decimal TaxAmount =>
        Math.Round(SubTotal * TaxRate / 100m, 2, MidpointRounding.AwayFromZero);

    public decimal Total => SubTotal + TaxAmount;
}

public class InvoiceLine
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    // Calculated value (not stored in the database)
    public decimal Amount => Math.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);
}
