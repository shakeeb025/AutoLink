using AutoLink.Shared.Enums;

namespace AutoLink.Shared.DTOs;

public class DealerProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string ContactPersonName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public DealerApprovalStatus ApprovalStatus { get; set; }
    public SubscriptionTier SubscriptionTier { get; set; }
    public DateTime SubscriptionExpiry { get; set; }
    public int ActiveListingsCount { get; set; }
    public int MaxListingLimit { get; set; }
    public double Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DealerApprovalDto
{
    public int DealerId { get; set; }
    public bool Approved { get; set; }
    public string? Remarks { get; set; }
}

public class ModerateListingDto
{
    public int VehicleId { get; set; }
    public VehicleStatus NewStatus { get; set; }
    public string? ModerationNotes { get; set; }
}

public class SubscriptionPlanDto
{
    public SubscriptionTier Tier { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public int MaxListings { get; set; }
    public bool PrioritySearch { get; set; }
    public bool FeaturedBadges { get; set; }
    public bool AdvancedAnalytics { get; set; }
    public bool StaffAccounts { get; set; }
    public List<string> Features { get; set; } = new();
}

public class DealerAnalyticsDto
{
    public int ActiveListings { get; set; }
    public int TotalViews { get; set; }
    public int TotalInquiries { get; set; }
    public int PendingTestDrives { get; set; }
    public int CompletedSalesThisMonth { get; set; }
    public decimal TotalSalesValue { get; set; }
    public double LeadConversionRate { get; set; }
    public List<MonthlyMetricDto> ViewsTrend { get; set; } = new();
    public List<MonthlyMetricDto> InquiriesTrend { get; set; } = new();
    public List<PopularVehicleMetricDto> TopVehicles { get; set; } = new();
}

public class MonthlyMetricDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class PopularVehicleMetricDto
{
    public int VehicleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Inquiries { get; set; }
}

public class PlatformStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalDealers { get; set; }
    public int PendingDealerApprovals { get; set; }
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int TotalTestDrives { get; set; }
    public int TotalInquiries { get; set; }
    public decimal MonthlySubscriptionRevenue { get; set; }
    public List<BrandDistributionDto> TopBrands { get; set; } = new();
    public List<MonthlyMetricDto> UserGrowthTrend { get; set; } = new();
}

public class BrandDistributionDto
{
    public string Brand { get; set; } = string.Empty;
    public int ListingCount { get; set; }
}
