using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AlarganShipping.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<PaymentReceipt> PaymentReceipts { get; set; }
        public DbSet<TrackingLog> TrackingLogs { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Transporter> Transporters { get; set; }
        public DbSet<DispatchOrder> DispatchOrders { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<ServiceRate> ServiceRates { get; set; }
        public DbSet<CarInspection> CarInspections { get; set; }
        public DbSet<DocumentAttachment> DocumentAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DispatchOrder>()
                .HasOne(d => d.OriginLocation)
                .WithMany(l => l.OriginDispatchOrders)
                .HasForeignKey(d => d.OriginLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DispatchOrder>()
                .HasOne(d => d.DestinationLocation)
                .WithMany(l => l.DestinationDispatchOrders)
                .HasForeignKey(d => d.DestinationLocationId)
                .OnDelete(DeleteBehavior.Restrict);


            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
}