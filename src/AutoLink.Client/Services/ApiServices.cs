using System.Net.Http.Json;
using AutoLink.Shared.DTOs;
using AutoLink.Shared.Enums;
using AutoLink.Shared.Models;

namespace AutoLink.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly LocalStorageService _localStorage;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, LocalStorageService localStorage, CustomAuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/auth/login", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            if (result != null && result.Success && result.Data != null)
            {
                await _localStorage.SetItemAsync("authToken", result.Data.Token);
                await _localStorage.SetItemAsync("currentUser", result.Data.User);
                _authStateProvider.NotifyUserAuthentication(result.Data.Token);
            }
            return result ?? ApiResponse<AuthResponseDto>.Fail("Unknown server response.");
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponseDto>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterCustomerAsync(RegisterCustomerDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/auth/register-customer", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            if (result != null && result.Success && result.Data != null)
            {
                await _localStorage.SetItemAsync("authToken", result.Data.Token);
                await _localStorage.SetItemAsync("currentUser", result.Data.User);
                _authStateProvider.NotifyUserAuthentication(result.Data.Token);
            }
            return result ?? ApiResponse<AuthResponseDto>.Fail("Unknown server response.");
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponseDto>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterSellerAsync(RegisterSellerDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/auth/register-seller", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
            if (result != null && result.Success && result.Data != null)
            {
                await _localStorage.SetItemAsync("authToken", result.Data.Token);
                await _localStorage.SetItemAsync("currentUser", result.Data.User);
                _authStateProvider.NotifyUserAuthentication(result.Data.Token);
            }
            return result ?? ApiResponse<AuthResponseDto>.Fail("Unknown server response.");
        }
        catch (Exception ex)
        {
            return ApiResponse<AuthResponseDto>.Fail(ex.Message);
        }
    }

    public async Task<UserInfoDto?> GetCurrentUserAsync()
    {
        return await _localStorage.GetItemAsync<UserInfoDto>("currentUser");
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("currentUser");
        _authStateProvider.NotifyUserLogout();
    }
}

public class VehicleService
{
    private readonly HttpClient _http;

    public VehicleService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<PagedResult<VehicleDto>>> GetVehiclesAsync(VehicleFilterDto filter)
    {
        try
        {
            var query = $"?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
            if (!string.IsNullOrEmpty(filter.SearchTerm)) query += $"&searchTerm={Uri.EscapeDataString(filter.SearchTerm)}";
            if (!string.IsNullOrEmpty(filter.Make)) query += $"&make={Uri.EscapeDataString(filter.Make)}";
            if (!string.IsNullOrEmpty(filter.Model)) query += $"&model={Uri.EscapeDataString(filter.Model)}";
            if (filter.MinPrice.HasValue) query += $"&minPrice={filter.MinPrice.Value}";
            if (filter.MaxPrice.HasValue) query += $"&maxPrice={filter.MaxPrice.Value}";
            if (filter.MinYear.HasValue) query += $"&minYear={filter.MinYear.Value}";
            if (filter.MaxYear.HasValue) query += $"&maxYear={filter.MaxYear.Value}";
            if (filter.MaxMileage.HasValue) query += $"&maxMileage={filter.MaxMileage.Value}";
            if (filter.BodyType.HasValue) query += $"&bodyType={(int)filter.BodyType.Value}";
            if (filter.FuelType.HasValue) query += $"&fuelType={(int)filter.FuelType.Value}";
            if (filter.Transmission.HasValue) query += $"&transmission={(int)filter.Transmission.Value}";
            if (!string.IsNullOrEmpty(filter.City)) query += $"&city={Uri.EscapeDataString(filter.City)}";
            if (!string.IsNullOrEmpty(filter.SortBy)) query += $"&sortBy={filter.SortBy}";

            var res = await _http.GetFromJsonAsync<ApiResponse<PagedResult<VehicleDto>>>($"api/vehicles{query}");
            return res ?? ApiResponse<PagedResult<VehicleDto>>.Fail("Error loading vehicles");
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<VehicleDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<VehicleDetailDto>> GetVehicleByIdAsync(int id)
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<VehicleDetailDto>>($"api/vehicles/{id}");
            return res ?? ApiResponse<VehicleDetailDto>.Fail("Error loading vehicle details");
        }
        catch (Exception ex)
        {
            return ApiResponse<VehicleDetailDto>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<VehicleDto>>> GetMyInventoryAsync(VehicleStatus? status = null)
    {
        try
        {
            string query = status.HasValue ? $"?status={(int)status.Value}" : "";
            var res = await _http.GetFromJsonAsync<ApiResponse<List<VehicleDto>>>($"api/vehicles/my-inventory{query}");
            return res ?? ApiResponse<List<VehicleDto>>.Fail("Error loading inventory");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<VehicleDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<VehicleDto>> CreateVehicleAsync(CreateVehicleDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/vehicles", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<VehicleDto>>();
            return result ?? ApiResponse<VehicleDto>.Fail("Error creating listing");
        }
        catch (Exception ex)
        {
            return ApiResponse<VehicleDto>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> UpdateVehicleAsync(int id, UpdateVehicleDto dto)
    {
        try
        {
            var res = await _http.PutAsJsonAsync($"api/vehicles/{id}", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("Error updating listing");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> UpdateVehicleStatusAsync(int id, VehicleStatus status)
    {
        try
        {
            var res = await _http.PatchAsJsonAsync($"api/vehicles/{id}/status", status);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("Error updating status");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> DeleteVehicleAsync(int id)
    {
        try
        {
            var res = await _http.DeleteAsync($"api/vehicles/{id}");
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("Error deleting vehicle");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<VehicleDto>>> GetFavoritesAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<VehicleDto>>>("api/favorites");
            return res ?? ApiResponse<List<VehicleDto>>.Fail("Error loading favorites");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<VehicleDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> ToggleFavoriteAsync(int vehicleId)
    {
        try
        {
            var res = await _http.PostAsync($"api/favorites/toggle/{vehicleId}", null);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            return result ?? ApiResponse<bool>.Fail("Error updating favorite");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail(ex.Message);
        }
    }
}

public class RecommendationService
{
    private readonly HttpClient _http;

    public RecommendationService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<List<VehicleMatchDto>>> GetRecommendationsAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<VehicleMatchDto>>>("api/recommendations");
            return res ?? ApiResponse<List<VehicleMatchDto>>.Fail("Error calculating recommendations");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<VehicleMatchDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<VehicleMatchDto>>> EvaluateCustomPreferencesAsync(CustomerPreferenceDto preferences)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/recommendations/evaluate", preferences);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<List<VehicleMatchDto>>>();
            return result ?? ApiResponse<List<VehicleMatchDto>>.Fail("Error evaluating custom preferences");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<VehicleMatchDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<CustomerPreferenceDto>> GetCustomerPreferencesAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<CustomerPreferenceDto>>("api/recommendations/preferences");
            return res ?? ApiResponse<CustomerPreferenceDto>.Fail("Error fetching preferences");
        }
        catch (Exception ex)
        {
            return ApiResponse<CustomerPreferenceDto>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<CustomerPreferenceDto>> SaveCustomerPreferencesAsync(CustomerPreferenceDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/recommendations/preferences", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<CustomerPreferenceDto>>();
            return result ?? ApiResponse<CustomerPreferenceDto>.Fail("Error saving preferences");
        }
        catch (Exception ex)
        {
            return ApiResponse<CustomerPreferenceDto>.Fail(ex.Message);
        }
    }
}

public class BookingService
{
    private readonly HttpClient _http;

    public BookingService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<TestDriveDto>> RequestTestDriveAsync(CreateTestDriveDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/testdrives", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<TestDriveDto>>();
            return result ?? ApiResponse<TestDriveDto>.Fail("Error booking test drive");
        }
        catch (Exception ex)
        {
            return ApiResponse<TestDriveDto>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<TestDriveDto>>> GetCustomerBookingsAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<TestDriveDto>>>("api/testdrives/my-bookings");
            return res ?? ApiResponse<List<TestDriveDto>>.Fail("Error loading test drives");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TestDriveDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<TestDriveDto>>> GetDealerBookingsAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<TestDriveDto>>>("api/testdrives/dealer-bookings");
            return res ?? ApiResponse<List<TestDriveDto>>.Fail("Error loading dealer bookings");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TestDriveDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> UpdateBookingStatusAsync(UpdateBookingStatusDto dto)
    {
        try
        {
            var res = await _http.PutAsJsonAsync("api/testdrives/status", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("Error updating status");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<LeadInquiryDto>> SubmitInquiryAsync(CreateInquiryDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/inquiries", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<LeadInquiryDto>>();
            return result ?? ApiResponse<LeadInquiryDto>.Fail("Error submitting inquiry");
        }
        catch (Exception ex)
        {
            return ApiResponse<LeadInquiryDto>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeadInquiryDto>>> GetCustomerInquiriesAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<LeadInquiryDto>>>("api/inquiries/my-inquiries");
            return res ?? ApiResponse<List<LeadInquiryDto>>.Fail("Error loading customer inquiries");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LeadInquiryDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeadInquiryDto>>> GetDealerInquiriesAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<LeadInquiryDto>>>("api/inquiries/dealer-inquiries");
            return res ?? ApiResponse<List<LeadInquiryDto>>.Fail("Error loading dealer inquiries");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LeadInquiryDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> UpdateInquiryStatusAsync(UpdateInquiryStatusDto dto)
    {
        try
        {
            var res = await _http.PutAsJsonAsync("api/inquiries/status", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("Error updating inquiry");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }
}

public class DealerService
{
    private readonly HttpClient _http;

    public DealerService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<List<DealerProfileDto>>> GetDealersAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<DealerProfileDto>>>("api/dealers");
            return res ?? ApiResponse<List<DealerProfileDto>>.Fail("Error loading dealerships");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<DealerProfileDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<DealerProfileDto>> GetMyProfileAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<DealerProfileDto>>("api/dealers/my-profile");
            return res ?? ApiResponse<DealerProfileDto>.Fail("Error loading profile");
        }
        catch (Exception ex)
        {
            return ApiResponse<DealerProfileDto>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<DealerProfileDto>> UpdateMyProfileAsync(DealerProfileDto dto)
    {
        try
        {
            var res = await _http.PutAsJsonAsync("api/dealers/my-profile", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<DealerProfileDto>>();
            return result ?? ApiResponse<DealerProfileDto>.Fail("Error updating profile");
        }
        catch (Exception ex)
        {
            return ApiResponse<DealerProfileDto>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<SubscriptionPlanDto>>> GetSubscriptionPlansAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<SubscriptionPlanDto>>>("api/dealers/subscription-plans");
            return res ?? ApiResponse<List<SubscriptionPlanDto>>.Fail("Error loading plans");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<SubscriptionPlanDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> UpgradeTierAsync(SubscriptionTier newTier)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/dealers/upgrade-tier", newTier);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("Error updating plan");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }
}

public class AdminService
{
    private readonly HttpClient _http;

    public AdminService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<PlatformStatsDto>> GetPlatformStatsAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<PlatformStatsDto>>("api/admin/stats");
            return res ?? ApiResponse<PlatformStatsDto>.Fail("Error loading stats");
        }
        catch (Exception ex)
        {
            return ApiResponse<PlatformStatsDto>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<DealerProfileDto>>> GetPendingSellersAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<DealerProfileDto>>>("api/admin/pending-sellers");
            return res ?? ApiResponse<List<DealerProfileDto>>.Fail("Error loading pending sellers");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<DealerProfileDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> ApproveSellerAsync(DealerApprovalDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/admin/sellers/approval", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("Error processing approval");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<UserInfoDto>>> GetUsersAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<List<UserInfoDto>>>("api/admin/users");
            return res ?? ApiResponse<List<UserInfoDto>>.Fail("Error loading users");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<UserInfoDto>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<string>> ModerateListingAsync(ModerateListingDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/admin/moderate-listing", dto);
            var result = await res.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("Error moderating listing");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<List<VehicleDto>>> GetAllListingsAsync(VehicleStatus? status = null)
    {
        try
        {
            string query = status.HasValue ? $"?status={(int)status.Value}" : "";
            var res = await _http.GetFromJsonAsync<ApiResponse<List<VehicleDto>>>($"api/admin/all-listings{query}");
            return res ?? ApiResponse<List<VehicleDto>>.Fail("Error loading listings");
        }
        catch (Exception ex)
        {
            return ApiResponse<List<VehicleDto>>.Fail(ex.Message);
        }
    }
}

public class AnalyticsService
{
    private readonly HttpClient _http;

    public AnalyticsService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<DealerAnalyticsDto>> GetDealerAnalyticsAsync()
    {
        try
        {
            var res = await _http.GetFromJsonAsync<ApiResponse<DealerAnalyticsDto>>("api/analytics/dealer");
            return res ?? ApiResponse<DealerAnalyticsDto>.Fail("Error loading dealer analytics");
        }
        catch (Exception ex)
        {
            return ApiResponse<DealerAnalyticsDto>.Fail(ex.Message);
        }
    }
}

public class ComparisonService
{
    private readonly List<VehicleDetailDto> _comparisonList = new();
    public event Action? OnComparisonChanged;

    public IReadOnlyList<VehicleDetailDto> Items => _comparisonList.AsReadOnly();
    public int Count => _comparisonList.Count;

    public bool Add(VehicleDetailDto vehicle)
    {
        if (_comparisonList.Count >= 4) return false;
        if (_comparisonList.Any(v => v.Id == vehicle.Id)) return false;

        _comparisonList.Add(vehicle);
        OnComparisonChanged?.Invoke();
        return true;
    }

    public bool Add(VehicleDto vehicle)
    {
        return Add(new VehicleDetailDto
        {
            Id = vehicle.Id,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Price = vehicle.Price,
            Mileage = vehicle.Mileage,
            BodyType = vehicle.BodyType,
            FuelType = vehicle.FuelType,
            Transmission = vehicle.Transmission,
            Color = vehicle.Color,
            DealerName = vehicle.DealerName,
            DealerCity = vehicle.DealerCity,
            Status = vehicle.Status,
            PrimaryImageUrl = vehicle.PrimaryImageUrl,
            IsFeatured = vehicle.IsFeatured,
            ViewsCount = vehicle.ViewsCount,
            CreatedAt = vehicle.CreatedAt
        });
    }

    public void Remove(int vehicleId)
    {
        var item = _comparisonList.FirstOrDefault(v => v.Id == vehicleId);
        if (item != null)
        {
            _comparisonList.Remove(item);
            OnComparisonChanged?.Invoke();
        }
    }

    public void Clear()
    {
        _comparisonList.Clear();
        OnComparisonChanged?.Invoke();
    }

    public bool IsInComparison(int vehicleId) => _comparisonList.Any(v => v.Id == vehicleId);
}
