using InvoiceManagementApi.Data;
using InvoiceManagementApi.Dtos;
using InvoiceManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagementApi.Services;

public interface IInvoiceService
{
    Task<ServiceResult<InvoiceResponse>> CreateAsync(CreateInvoiceRequest request, CancellationToken ct);
    Task<InvoiceResponse?> GetAsync(int id, CancellationToken ct);
    Task<List<InvoiceResponse>> ListAsync(InvoiceStatus? status, int? shipmentId, CancellationToken ct);
    Task<ServiceResult<InvoiceResponse>> IssueAsync(int id, CancellationToken ct);
    Task<ServiceResult<InvoiceResponse>> MarkPaidAsync(int id, CancellationToken ct);
}

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;

    public InvoiceService(AppDbContext db) => _db = db;

    // Loads an invoice together with its shipment and lines
    private IQueryable<Invoice> InvoicesWithDetails() =>
        _db.Invoices
            .Include(i => i.Shipment)
            .Include(i => i.Lines);

    public async Task<ServiceResult<InvoiceResponse>> CreateAsync(
        CreateInvoiceRequest request, CancellationToken ct)
    {
        var shipment = await _db.Shipments.FirstOrDefaultAsync(s => s.Id == request.ShipmentId, ct);
        if (shipment is null)
            return ServiceResult<InvoiceResponse>.NotFound($"Shipment {request.ShipmentId} was not found.");

        var nextId = (await _db.Invoices.MaxAsync(i => (int?)i.Id, ct) ?? 0) + 1;

        var invoice = new Invoice
        {
            InvoiceNo = $"INV-{DateTime.UtcNow:yyyyMM}-{nextId:D5}",
            ShipmentId = shipment.Id,
            Shipment = shipment,
            Currency = string.IsNullOrWhiteSpace(request.Currency)
                ? "INR"
                : request.Currency.Trim().ToUpperInvariant(),
            TaxRate = request.TaxRate,
            Status = InvoiceStatus.Draft,
            Lines = request.Lines.Select(l => new InvoiceLine
            {
                Description = l.Description.Trim(),
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        return ServiceResult<InvoiceResponse>.Ok(invoice.ToResponse());
    }

    public async Task<InvoiceResponse?> GetAsync(int id, CancellationToken ct)
    {
        var invoice = await InvoicesWithDetails()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        return invoice?.ToResponse();
    }

    public async Task<List<InvoiceResponse>> ListAsync(
        InvoiceStatus? status, int? shipmentId, CancellationToken ct)
    {
        var query = InvoicesWithDetails().AsNoTracking();

        if (status is not null)
            query = query.Where(i => i.Status == status);

        if (shipmentId is not null)
            query = query.Where(i => i.ShipmentId == shipmentId);

        var invoices = await query.OrderByDescending(i => i.Id).ToListAsync(ct);
        return invoices.Select(i => i.ToResponse()).ToList();
    }

    public Task<ServiceResult<InvoiceResponse>> IssueAsync(int id, CancellationToken ct) =>
        ChangeStatusAsync(id, InvoiceStatus.Draft, InvoiceStatus.Issued, ct);

    public Task<ServiceResult<InvoiceResponse>> MarkPaidAsync(int id, CancellationToken ct) =>
        ChangeStatusAsync(id, InvoiceStatus.Issued, InvoiceStatus.Paid, ct);

    // Business rule: Draft -> Issued -> Paid, in that order only
    private async Task<ServiceResult<InvoiceResponse>> ChangeStatusAsync(
        int id, InvoiceStatus required, InvoiceStatus target, CancellationToken ct)
    {
        var invoice = await InvoicesWithDetails().FirstOrDefaultAsync(i => i.Id == id, ct);
        if (invoice is null)
            return ServiceResult<InvoiceResponse>.NotFound($"Invoice {id} was not found.");

        if (invoice.Status != required)
            return ServiceResult<InvoiceResponse>.Conflict(
                $"Invoice must be {required} to become {target}, but it is {invoice.Status}.");

        invoice.Status = target;
        await _db.SaveChangesAsync(ct);

        return ServiceResult<InvoiceResponse>.Ok(invoice.ToResponse());
    }
}
