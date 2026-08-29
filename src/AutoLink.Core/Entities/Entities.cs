using Microsoft.AspNetCore.Identity;
using AutoLink.Shared.Enums;

namespace AutoLink.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation
    public DealerProfile? DealerProfile { get; set; }
    public CustomerPreference? Preference { get; set; }
    public ICollection<FavoriteListing> Favorites { get; set; } = new List<FavoriteListing>();
    public ICollection<TestDriveBooking> TestDrives { get; set; } = new List<TestDriveBooking>();
    public ICollection<LeadInquiry> Inquiries { get; set; } = new List<LeadInquiry>();
}

public class DealerProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public string BusinessName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    
    public DealerApprovalStatus ApprovalStatus { get; set; } = DealerApprovalStatus.Pending;
    public string? ApprovalRemarks { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public SubscriptionTier SubscriptionTier { get; set; } = SubscriptionTier.Free;
    public DateTime SubscriptionExpiry { get; set; } = DateTime.UtcNow.AddYears(1);
    public int MaxListingLimit { get; set; } = 5; // 5 for Free, 25 for Standard, 100 for Premium
    public double Rating { get; set; } = 4.8;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<VehicleListing> Listings { get; set; } = new List<VehicleListing>();
    public ICollection<TestDriveBooking> TestDrives { get; set; } = new List<TestDriveBooking>();
    public ICollection<LeadInquiry> Inquiries { get; set; } = new List<LeadInquiry>();
}

public class VehicleListing
{
    public int Id { get; set; }
    public int DealerId { get; set; }
    public DealerProfile Dealer { get; set; } = null!;

    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public BodyType BodyType { get; set; } = BodyType.Sedan;
    public FuelType FuelType { get; set; } = FuelType.Petrol;
    public TransmissionType Transmission { get; set; } = TransmissionType.Automatic;
    public string Color { get; set; } = string.Empty;
    public string EngineCapacity { get; set; } = string.Empty;
    public int Horsepower { get; set; }
    public int SeatingCapacity { get; set; } = 5;
    public string Vin { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FeaturesJson { get; set; } = "[]";
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    public int ViewsCount { get; set; }
    public bool IsFeatured { get; set; }
    public string? ModerationNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();
    public ICollection<FavoriteListing> FavoritedBy { get; set; } = new List<FavoriteListing>();
    public ICollection<TestDriveBooking> TestDrives { get; set; } = new List<TestDriveBooking>();
    public ICollection<LeadInquiry> Inquiries { get; set; } = new List<LeadInquiry>();
}

public class VehicleImage
{
    public int Id { get; set; }
    public int VehicleListingId { get; set; }
    public VehicleListing VehicleListing { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class CustomerPreference
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public ApplicationUser Customer { get; set; } = null!;

    public decimal? MinBudget { get; set; }
    public decimal? MaxBudget { get; set; }
    public string PreferredMakesJson { get; set; } = "[]";
    public string PreferredBodyTypesJson { get; set; } = "[]";
    public string PreferredFuelTypesJson { get; set; } = "[]";
    public string PreferredTransmissionsJson { get; set; } = "[]";
    public int? MinYear { get; set; }
    public int? MaxMileage { get; set; }
    public string? PreferredCity { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class TestDriveBooking
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public VehicleListing Vehicle { get; set; } = null!;

    public int DealerId { get; set; }
    public DealerProfile Dealer { get; set; } = null!;

    public string CustomerId { get; set; } = string.Empty;
    public ApplicationUser Customer { get; set; } = null!;

    public DateTime ScheduledDate { get; set; }
    public string PreferredTimeSlot { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string CustomerContactNumber { get; set; } = string.Empty;
    public BookingStatus Status { get; set; } = BookingStatus.Requested;
    public string? DealerNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class LeadInquiry
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public VehicleListing Vehicle { get; set; } = null!;

    public int DealerId { get; set; }
    public DealerProfile Dealer { get; set; } = null!;

    public string? CustomerId { get; set; }
    public ApplicationUser? Customer { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public InquiryStatus Status { get; set; } = InquiryStatus.New;
    public string? DealerResponse { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class FavoriteListing
{
    public string CustomerId { get; set; } = string.Empty;
    public ApplicationUser Customer { get; set; } = null!;

    public int VehicleId { get; set; }
    public VehicleListing Vehicle { get; set; } = null!;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}

public class SubscriptionPlan
{
    public int Id { get; set; }
    public SubscriptionTier Tier { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public int MaxListings { get; set; }
    public bool PrioritySearch { get; set; }
    public bool FeaturedBadges { get; set; }
    public bool AdvancedAnalytics { get; set; }
    public bool StaffAccounts { get; set; }
    public string FeaturesJson { get; set; } = "[]";
}
