using InvoiceManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagementApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>(e =>
        {
            e.HasIndex(s => s.ReferenceNo).IsUnique();
            e.Property(s => s.ReferenceNo).HasMaxLength(30).IsRequired();
            e.Property(s => s.Customer).HasMaxLength(150).IsRequired();
            e.Property(s => s.Origin).HasMaxLength(100).IsRequired();
            e.Property(s => s.Destination).HasMaxLength(100).IsRequired();
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasIndex(i => i.InvoiceNo).IsUnique();
            e.Property(i => i.InvoiceNo).HasMaxLength(30).IsRequired();
            e.Property(i => i.Currency).HasMaxLength(3).IsRequired();
            e.Property(i => i.TaxRate).HasPrecision(5, 2);
            e.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(i => i.Shipment)
             .WithMany(s => s.Invoices)
             .HasForeignKey(i => i.ShipmentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(i => i.Lines)
             .WithOne(l => l.Invoice)
             .HasForeignKey(l => l.InvoiceId)
             .OnDelete(DeleteBehavior.Cascade);

            // Calculated properties are not columns
            e.Ignore(i => i.SubTotal);
            e.Ignore(i => i.TaxAmount);
            e.Ignore(i => i.Total);
        });

        modelBuilder.Entity<InvoiceLine>(e =>
        {
            e.Property(l => l.Description).HasMaxLength(200).IsRequired();
            e.Property(l => l.Quantity).HasPrecision(18, 2);
            e.Property(l => l.UnitPrice).HasPrecision(18, 2);
            e.Ignore(l => l.Amount);
        });
    }
}
