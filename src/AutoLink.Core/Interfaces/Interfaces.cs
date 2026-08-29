using AutoLink.Core.Entities;
using AutoLink.Shared.DTOs;
using AutoLink.Shared.Enums;
using AutoLink.Shared.Models;

namespace AutoLink.Core.Interfaces;

public interface IVehicleRepository
{
    Task<PagedResult<VehicleDto>> GetVehiclesAsync(VehicleFilterDto filter, string? currentUserId = null);
    Task<VehicleListing?> GetByIdAsync(int id);
    Task<VehicleDetailDto?> GetDetailByIdAsync(int id, string? currentUserId = null);
    Task<List<VehicleListing>> GetAllAvailableListingsAsync();
    Task<List<VehicleDto>> GetSellerListingsAsync(int dealerId, VehicleStatus? status = null);
    Task<VehicleListing> AddAsync(VehicleListing vehicle);
    Task UpdateAsync(VehicleListing vehicle);
    Task DeleteAsync(int id);
    Task IncrementViewsAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<List<FavoriteListing>> GetCustomerFavoritesAsync(string customerId);
    Task<bool> ToggleFavoriteAsync(string customerId, int vehicleId);
    Task<bool> IsFavoriteAsync(string customerId, int vehicleId);
}

public interface IDealerRepository
{
    Task<DealerProfile?> GetByIdAsync(int id);
    Task<DealerProfile?> GetByUserIdAsync(string userId);
    Task<List<DealerProfileDto>> GetAllDealersAsync(DealerApprovalStatus? status = null);
    Task<List<DealerProfileDto>> GetPendingApprovalDealersAsync();
    Task<DealerProfile> AddAsync(DealerProfile dealer);
    Task UpdateAsync(DealerProfile dealer);
    Task<bool> ApproveDealerAsync(int dealerId, bool approved, string? remarks);
    Task<DealerAnalyticsDto> GetDealerAnalyticsAsync(int dealerId);
}

public interface ITestDriveRepository
{
    Task<TestDriveBooking?> GetByIdAsync(int id);
    Task<List<TestDriveDto>> GetCustomerBookingsAsync(string customerId);
    Task<List<TestDriveDto>> GetDealerBookingsAsync(int dealerId);
    Task<TestDriveBooking> AddAsync(TestDriveBooking booking);
    Task UpdateStatusAsync(int bookingId, BookingStatus status, string? dealerNotes);
}

public interface IInquiryRepository
{
    Task<LeadInquiry?> GetByIdAsync(int id);
    Task<List<LeadInquiryDto>> GetCustomerInquiriesAsync(string customerId);
    Task<List<LeadInquiryDto>> GetDealerInquiriesAsync(int dealerId);
    Task<LeadInquiry> AddAsync(LeadInquiry inquiry);
    Task UpdateStatusAsync(int inquiryId, InquiryStatus status, string? dealerResponse);
}

public interface ICustomerPreferenceRepository
{
    Task<CustomerPreference?> GetByCustomerIdAsync(string customerId);
    Task<CustomerPreference> SavePreferenceAsync(string customerId, CustomerPreferenceDto dto);
}

public interface IRecommendationService
{
    Task<IEnumerable<VehicleMatchDto>> GetRecommendationsAsync(string customerId);
    Task<IEnumerable<VehicleMatchDto>> GetRecommendationsWithCustomPreferencesAsync(CustomerPreferenceDto preferences);
    MatchBreakdownDto CalculateMatchScore(VehicleListing vehicle, CustomerPreferenceDto preferences);
}

public interface ITokenService
{
    Task<AuthResponseDto> GenerateTokenAsync(ApplicationUser user, DealerProfile? dealer = null);
}

public interface IAdminService
{
    Task<PlatformStatsDto> GetPlatformStatsAsync();
    Task<List<UserInfoDto>> GetAllUsersAsync();
    Task<bool> ModerateListingAsync(int vehicleId, VehicleStatus newStatus, string? moderationNotes);
}
