using InvoiceManagementApi.Data;
using InvoiceManagementApi.Dtos;
using InvoiceManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagementApi.Services;

public interface IShipmentService
{
    Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, CancellationToken ct);
    Task<ShipmentResponse?> GetAsync(int id, CancellationToken ct);
    Task<List<ShipmentResponse>> ListAsync(ShipmentStatus? status, CancellationToken ct);
    Task<ServiceResult<ShipmentResponse>> UpdateStatusAsync(int id, ShipmentStatus newStatus, CancellationToken ct);
}

public class ShipmentService : IShipmentService
{
    private readonly AppDbContext _db;

    // AppDbContext is injected by the built-in dependency injection container
    public ShipmentService(AppDbContext db) => _db = db;

    public async Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request, CancellationToken ct)
    {
        var nextId = (await _db.Shipments.MaxAsync(s => (int?)s.Id, ct) ?? 0) + 1;

        var shipment = new Shipment
        {
            ReferenceNo = $"SHP-{DateTime.UtcNow:yyyyMM}-{nextId:D5}",
            Customer = request.Customer.Trim(),
            Origin = request.Origin.Trim(),
            Destination = request.Destination.Trim(),
            Status = ShipmentStatus.Booked
        };

        _db.Shipments.Add(shipment);
        await _db.SaveChangesAsync(ct);

        return shipment.ToResponse();
    }

    public async Task<ShipmentResponse?> GetAsync(int id, CancellationToken ct)
    {
        var shipment = await _db.Shipments
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return shipment?.ToResponse();
    }

    public async Task<List<ShipmentResponse>> ListAsync(ShipmentStatus? status, CancellationToken ct)
    {
        var query = _db.Shipments.AsNoTracking();

        if (status is not null)
            query = query.Where(s => s.Status == status);

        var shipments = await query.OrderByDescending(s => s.Id).ToListAsync(ct);
        return shipments.Select(s => s.ToResponse()).ToList();
    }

    public async Task<ServiceResult<ShipmentResponse>> UpdateStatusAsync(
        int id, ShipmentStatus newStatus, CancellationToken ct)
    {
        var shipment = await _db.Shipments.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (shipment is null)
            return ServiceResult<ShipmentResponse>.NotFound($"Shipment {id} was not found.");

        // Business rule: status can only move forward (Booked -> InTransit -> Delivered)
        if (newStatus <= shipment.Status)
            return ServiceResult<ShipmentResponse>.Conflict(
                $"Cannot change status from {shipment.Status} to {newStatus}.");

        shipment.Status = newStatus;
        await _db.SaveChangesAsync(ct);

        return ServiceResult<ShipmentResponse>.Ok(shipment.ToResponse());
    }
}
