using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoLink.Core.Entities;

namespace AutoLink.Infrastructure.Data;

public class AutoLinkDbContext : IdentityDbContext<ApplicationUser>
{
    public AutoLinkDbContext(DbContextOptions<AutoLinkDbContext> options) : base(options)
    {
    }

    public DbSet<DealerProfile> DealerProfiles => Set<DealerProfile>();
    public DbSet<VehicleListing> VehicleListings => Set<VehicleListing>();
    public DbSet<VehicleImage> VehicleImages => Set<VehicleImage>();
    public DbSet<CustomerPreference> CustomerPreferences => Set<CustomerPreference>();
    public DbSet<TestDriveBooking> TestDriveBookings => Set<TestDriveBooking>();
    public DbSet<LeadInquiry> LeadInquiries => Set<LeadInquiry>();
    public DbSet<FavoriteListing> FavoriteListings => Set<FavoriteListing>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser to DealerProfile (1:1 optional)
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.DealerProfile)
            .WithOne(d => d.User)
            .HasForeignKey<DealerProfile>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApplicationUser to CustomerPreference (1:1 optional)
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Preference)
            .WithOne(p => p.Customer)
            .HasForeignKey<CustomerPreference>(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // DealerProfile to VehicleListings (1:N)
        builder.Entity<DealerProfile>()
            .HasMany(d => d.Listings)
            .WithOne(v => v.Dealer)
            .HasForeignKey(v => v.DealerId)
            .OnDelete(DeleteBehavior.Cascade);

        // VehicleListing to VehicleImages (1:N)
        builder.Entity<VehicleListing>()
            .HasMany(v => v.Images)
            .WithOne(i => i.VehicleListing)
            .HasForeignKey(i => i.VehicleListingId)
            .OnDelete(DeleteBehavior.Cascade);

        // FavoriteListing (Composite Key)
        builder.Entity<FavoriteListing>()
            .HasKey(f => new { f.CustomerId, f.VehicleId });

        builder.Entity<FavoriteListing>()
            .HasOne(f => f.Customer)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FavoriteListing>()
            .HasOne(f => f.Vehicle)
            .WithMany(v => v.FavoritedBy)
            .HasForeignKey(f => f.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        // TestDriveBooking
        builder.Entity<TestDriveBooking>()
            .HasOne(t => t.Vehicle)
            .WithMany(v => v.TestDrives)
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TestDriveBooking>()
            .HasOne(t => t.Dealer)
            .WithMany(d => d.TestDrives)
            .HasForeignKey(t => t.DealerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TestDriveBooking>()
            .HasOne(t => t.Customer)
            .WithMany(u => u.TestDrives)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // LeadInquiry
        builder.Entity<LeadInquiry>()
            .HasOne(l => l.Vehicle)
            .WithMany(v => v.Inquiries)
            .HasForeignKey(l => l.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeadInquiry>()
            .HasOne(l => l.Dealer)
            .WithMany(d => d.Inquiries)
            .HasForeignKey(l => l.DealerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LeadInquiry>()
            .HasOne(l => l.Customer)
            .WithMany(u => u.Inquiries)
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Decimals Precision
        builder.Entity<VehicleListing>()
            .Property(v => v.Price)
            .HasPrecision(18, 2);

        builder.Entity<SubscriptionPlan>()
            .Property(s => s.MonthlyPrice)
            .HasPrecision(18, 2);

        builder.Entity<CustomerPreference>()
            .Property(p => p.MinBudget)
            .HasPrecision(18, 2);

        builder.Entity<CustomerPreference>()
            .Property(p => p.MaxBudget)
            .HasPrecision(18, 2);

        // Search Indexes
        builder.Entity<VehicleListing>()
            .HasIndex(v => new { v.Make, v.Model, v.Year, v.Price, v.Status });

        builder.Entity<VehicleListing>()
            .HasIndex(v => v.DealerId);
    }
}
