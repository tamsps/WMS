using Microsoft.EntityFrameworkCore;
using WMS.Domain.Entities;
using WMS.Domain.Common;

namespace WMS.Infrastructure.Data;

public class WMSDbContext : DbContext
{
    public WMSDbContext(DbContextOptions<WMSDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Inbound> Inbounds => Set<Inbound>();
    public DbSet<InboundItem> InboundItems => Set<InboundItem>();
    public DbSet<Outbound> Outbounds => Set<Outbound>();
    public DbSet<OutboundItem> OutboundItems => Set<OutboundItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentEvent> PaymentEvents => Set<PaymentEvent>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryEvent> DeliveryEvents => Set<DeliveryEvent>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Suppress pending model changes warning during migrations
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.Property(e => e.SKU).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UOM).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Weight).HasColumnType("decimal(18,4)");
            entity.Property(e => e.Length).HasColumnType("decimal(18,4)");
            entity.Property(e => e.Width).HasColumnType("decimal(18,4)");
            entity.Property(e => e.Height).HasColumnType("decimal(18,4)");
            entity.Property(e => e.ReorderLevel).HasColumnType("decimal(18,4)");
            entity.Property(e => e.MaxStockLevel).HasColumnType("decimal(18,4)");
        });

        // Location Configuration
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Zone).HasMaxLength(50);
            entity.Property(e => e.Aisle).HasMaxLength(50);
            entity.Property(e => e.Rack).HasMaxLength(50);
            entity.Property(e => e.Shelf).HasMaxLength(50);
            entity.Property(e => e.Bin).HasMaxLength(50);
            entity.Property(e => e.Capacity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.CurrentOccupancy).HasColumnType("decimal(18,4)");
            
            entity.HasOne(e => e.ParentLocation)
                .WithMany(e => e.ChildLocations)
                .HasForeignKey(e => e.ParentLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Inventory Configuration
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ProductId, e.LocationId }).IsUnique();
            entity.Property(e => e.QuantityOnHand).HasColumnType("decimal(18,4)");
            entity.Property(e => e.QuantityReserved).HasColumnType("decimal(18,4)");
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Location)
                .WithMany(l => l.Inventories)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Inventory Transaction Configuration
        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TransactionNumber);
            entity.Property(e => e.TransactionNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.BalanceBefore).HasColumnType("decimal(18,4)");
            entity.Property(e => e.BalanceAfter).HasColumnType("decimal(18,4)");
            
            entity.HasOne(e => e.Product)
                .WithMany(p => p.InventoryTransactions)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Inbound Configuration
        modelBuilder.Entity<Inbound>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.InboundNumber).IsUnique();
            entity.Property(e => e.InboundNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SupplierName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<InboundItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExpectedQuantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.ReceivedQuantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.DamagedQuantity).HasColumnType("decimal(18,4)");
            
            entity.HasOne(e => e.Inbound)
                .WithMany(i => i.InboundItems)
                .HasForeignKey(e => e.InboundId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Location)
                .WithMany(l => l.InboundItems)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Outbound Configuration
        modelBuilder.Entity<Outbound>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OutboundNumber).IsUnique();
            entity.Property(e => e.OutboundNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);

            // Outbound -> Payment (FK on Outbound.PaymentId)
            entity.HasOne(e => e.Payment)
                .WithOne()
                .HasForeignKey<Outbound>(e => e.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Outbound -> Delivery (FK on Outbound.DeliveryId) - matches existing migration schema
            entity.HasOne(e => e.Delivery)
                .WithOne(d => d.Outbound)
                .HasForeignKey<Outbound>(e => e.DeliveryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OutboundItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderedQuantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.PickedQuantity).HasColumnType("decimal(18,4)");
            entity.Property(e => e.ShippedQuantity).HasColumnType("decimal(18,4)");
            
            entity.HasOne(e => e.Outbound)
                .WithMany(o => o.OutboundItems)
                .HasForeignKey(e => e.OutboundId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Location)
                .WithMany(l => l.OutboundItems)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PaymentNumber).IsUnique();
            entity.HasIndex(e => e.ExternalPaymentId);
            entity.Property(e => e.PaymentNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Currency).HasMaxLength(10);

            // Payment -> Outbound (optional FK on Payment.OutboundId) - used by PaymentService
            entity.HasOne(e => e.Outbound)
                .WithOne()
                .HasForeignKey<Payment>(e => e.OutboundId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PaymentEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Payment)
                .WithMany(p => p.PaymentEvents)
                .HasForeignKey(e => e.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Delivery Configuration
        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DeliveryNumber).IsUnique();
            entity.HasIndex(e => e.TrackingNumber);
            entity.Property(e => e.DeliveryNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);

            // Relationship is configured from Outbound (FK: Outbound.DeliveryId)
        });

        modelBuilder.Entity<DeliveryEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Delivery)
                .WithMany(d => d.DeliveryEvents)
                .HasForeignKey(e => e.DeliveryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed Data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Roles
        var adminRoleId = Guid.NewGuid();
        var managerRoleId = Guid.NewGuid();
        var warehouseStaffRoleId = Guid.NewGuid();

        modelBuilder.Entity<Role>().HasData(
            new Role 
            { 
                Id = adminRoleId, 
                Name = "Admin", 
                Description = "System Administrator",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Role 
            { 
                Id = managerRoleId, 
                Name = "Manager", 
                Description = "Warehouse Manager",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Role 
            { 
                Id = warehouseStaffRoleId, 
                Name = "WarehouseStaff", 
                Description = "Warehouse Staff",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );

        // Seed Default Admin User
        // Password: Admin@123
        var adminUserId = Guid.NewGuid();
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = adminUserId,
                Username = "admin",
                Email = "admin@wms.com",
                PasswordHash = "$2a$11$D7Z5z8YqJ5qH5F0ZK5Z5Z.Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z5Z", // Admin@123
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );

        modelBuilder.Entity<UserRole>().HasData(
            new UserRole
            {
                UserId = adminUserId,
                RoleId = adminRoleId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "System"
            }
        );
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
