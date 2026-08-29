using System.Text.Json;
using AutoLink.Core.Entities;
using AutoLink.Core.Interfaces;
using AutoLink.Shared.DTOs;
using AutoLink.Shared.Enums;

namespace AutoLink.Core.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ICustomerPreferenceRepository _preferenceRepository;

    public RecommendationService(
        IVehicleRepository vehicleRepository,
        ICustomerPreferenceRepository preferenceRepository)
    {
        _vehicleRepository = vehicleRepository;
        _preferenceRepository = preferenceRepository;
    }

    public async Task<IEnumerable<VehicleMatchDto>> GetRecommendationsAsync(string customerId)
    {
        var pref = await _preferenceRepository.GetByCustomerIdAsync(customerId);
        var prefDto = MapToDto(pref, customerId);
        return await GetRecommendationsWithCustomPreferencesAsync(prefDto);
    }

    public async Task<IEnumerable<VehicleMatchDto>> GetRecommendationsWithCustomPreferencesAsync(CustomerPreferenceDto preferences)
    {
        var vehicles = await _vehicleRepository.GetAllAvailableListingsAsync();
        var matches = new List<VehicleMatchDto>();

        foreach (var vehicle in vehicles)
        {
            var breakdown = CalculateMatchScore(vehicle, preferences);
            double totalScore = breakdown.BudgetScore 
                              + breakdown.BodyTypeScore 
                              + breakdown.FuelTransmissionScore 
                              + breakdown.YearScore 
                              + breakdown.MileageScore;

            totalScore = Math.Round(Math.Clamp(totalScore, 0.0, 100.0), 1);

            var primaryImg = vehicle.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                             ?? vehicle.Images.FirstOrDefault()?.ImageUrl
                             ?? "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800";

            var vehicleDto = new VehicleDto
            {
                Id = vehicle.Id,
                DealerId = vehicle.DealerId,
                DealerName = vehicle.Dealer?.BusinessName ?? "AutoLink Certified Dealer",
                DealerCity = vehicle.Dealer?.City ?? "Verified Dealer",
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Price = vehicle.Price,
                Mileage = vehicle.Mileage,
                BodyType = vehicle.BodyType,
                FuelType = vehicle.FuelType,
                Transmission = vehicle.Transmission,
                Color = vehicle.Color,
                Status = vehicle.Status,
                PrimaryImageUrl = primaryImg,
                ViewsCount = vehicle.ViewsCount,
                CreatedAt = vehicle.CreatedAt,
                IsFeatured = vehicle.IsFeatured,
                Images = vehicle.Images.Select(i => new VehicleImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder
                }).ToList()
            };

            matches.Add(new VehicleMatchDto
            {
                Vehicle = vehicleDto,
                MatchPercentage = totalScore,
                Breakdown = breakdown
            });
        }

        // Return listings ordered by ranking match percentage descending
        return matches.OrderByDescending(m => m.MatchPercentage).ThenBy(m => m.Vehicle.Price);
    }

    public MatchBreakdownDto CalculateMatchScore(VehicleListing vehicle, CustomerPreferenceDto preferences)
    {
        var breakdown = new MatchBreakdownDto();
        var reasons = new List<string>();

        // 1. Budget Match (Weight: 30%)
        double budgetScore = 0.0;
        decimal? minB = preferences.MinBudget;
        decimal? maxB = preferences.MaxBudget;

        if (minB.HasValue || maxB.HasValue)
        {
            decimal minVal = minB ?? 0;
            decimal maxVal = maxB ?? decimal.MaxValue;

            if (vehicle.Price >= minVal && vehicle.Price <= maxVal)
            {
                budgetScore = 30.0;
                reasons.Add($"Price (${vehicle.Price:N0}) is within your targeted budget.");
            }
            else if (vehicle.Price > maxVal && maxVal > 0)
            {
                // Smooth penalty for exceeding maximum budget
                decimal excess = vehicle.Price - maxVal;
                decimal allowance = maxVal * 0.30m; // 30% flexibility cushion
                double penaltyRatio = (double)Math.Min(excess / (allowance > 0 ? allowance : 1), 1.0m);
                budgetScore = Math.Max(0.0, 30.0 * (1.0 - penaltyRatio));
                if (budgetScore > 10.0)
                {
                    reasons.Add($"Slightly above maximum budget (${vehicle.Price:N0}).");
                }
            }
            else if (vehicle.Price < minVal && minVal > 0)
            {
                // Price is under minimum budget (bargain)
                budgetScore = 28.0;
                reasons.Add($"Great value! Price (${vehicle.Price:N0}) is below your budget range.");
            }
        }
        else
        {
            budgetScore = 25.0; // Baseline when unconfigured
        }
        breakdown.BudgetScore = Math.Round(budgetScore, 1);

        // 2. Body Type Match (Weight: 20%)
        double bodyScore = 0.0;
        if (preferences.PreferredBodyTypes != null && preferences.PreferredBodyTypes.Count > 0)
        {
            if (preferences.PreferredBodyTypes.Contains(vehicle.BodyType))
            {
                bodyScore = 20.0;
                reasons.Add($"Matches your desired {vehicle.BodyType} body style.");
            }
            else
            {
                bodyScore = 0.0;
            }
        }
        else
        {
            bodyScore = 15.0; // Baseline neutral
        }
        breakdown.BodyTypeScore = Math.Round(bodyScore, 1);

        // 3. Fuel & Transmission Match (Weight: 20% -> 10% Fuel + 10% Transmission)
        double fuelScore = 0.0;
        if (preferences.PreferredFuelTypes != null && preferences.PreferredFuelTypes.Count > 0)
        {
            if (preferences.PreferredFuelTypes.Contains(vehicle.FuelType))
            {
                fuelScore = 10.0;
                reasons.Add($"Matches preferred fuel type ({vehicle.FuelType}).");
            }
            else
            {
                fuelScore = 0.0;
            }
        }
        else
        {
            fuelScore = 7.5;
        }

        double transScore = 0.0;
        if (preferences.PreferredTransmissions != null && preferences.PreferredTransmissions.Count > 0)
        {
            if (preferences.PreferredTransmissions.Contains(vehicle.Transmission))
            {
                transScore = 10.0;
                reasons.Add($"Matches preferred transmission ({vehicle.Transmission}).");
            }
            else
            {
                transScore = 0.0;
            }
        }
        else
        {
            transScore = 7.5;
        }
        breakdown.FuelTransmissionScore = Math.Round(fuelScore + transScore, 1);

        // 4. Manufacturing Year Range Match (Weight: 15%)
        double yearScore = 0.0;
        if (preferences.MinYear.HasValue && preferences.MinYear.Value > 0)
        {
            int minYr = preferences.MinYear.Value;
            if (vehicle.Year >= minYr)
            {
                yearScore = 15.0;
                reasons.Add($"Model year {vehicle.Year} meets your minimum year requirement.");
            }
            else
            {
                int diff = minYr - vehicle.Year;
                double decay = Math.Max(0.0, 1.0 - (diff * 0.25)); // decays over 4 years
                yearScore = 15.0 * decay;
            }
        }
        else
        {
            int currentYear = DateTime.UtcNow.Year;
            int age = Math.Max(0, currentYear - vehicle.Year);
            yearScore = age <= 3 ? 15.0 : age <= 7 ? 12.0 : 8.0;
        }
        breakdown.YearScore = Math.Round(yearScore, 1);

        // 5. Mileage Range Match (Weight: 15%)
        double mileageScore = 0.0;
        if (preferences.MaxMileage.HasValue && preferences.MaxMileage.Value > 0)
        {
            int maxMil = preferences.MaxMileage.Value;
            if (vehicle.Mileage <= maxMil)
            {
                mileageScore = 15.0;
                reasons.Add($"Low mileage ({vehicle.Mileage:N0} mi) within your desired limit.");
            }
            else
            {
                int excess = vehicle.Mileage - maxMil;
                double decay = Math.Max(0.0, 1.0 - ((double)excess / (maxMil * 0.5)));
                mileageScore = 15.0 * decay;
            }
        }
        else
        {
            mileageScore = vehicle.Mileage < 30000 ? 15.0 : vehicle.Mileage < 75000 ? 12.0 : 8.0;
        }
        breakdown.MileageScore = Math.Round(mileageScore, 1);

        // Preferred Make Bonus highlight
        if (preferences.PreferredMakes != null && preferences.PreferredMakes.Any(m => m.Equals(vehicle.Make, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Insert(0, $"Preferred Brand: {vehicle.Make}");
        }

        breakdown.MatchReasons = reasons;
        return breakdown;
    }

    private static CustomerPreferenceDto MapToDto(CustomerPreference? pref, string customerId)
    {
        if (pref == null)
        {
            return new CustomerPreferenceDto
            {
                CustomerId = customerId,
                MinBudget = null,
                MaxBudget = null,
                PreferredMakes = new(),
                PreferredBodyTypes = new(),
                PreferredFuelTypes = new(),
                PreferredTransmissions = new(),
                MinYear = null,
                MaxMileage = null,
                UpdatedAt = DateTime.UtcNow
            };
        }

        var makes = SafeDeserialize<List<string>>(pref.PreferredMakesJson);
        var bodies = SafeDeserialize<List<BodyType>>(pref.PreferredBodyTypesJson);
        var fuels = SafeDeserialize<List<FuelType>>(pref.PreferredFuelTypesJson);
        var transmissions = SafeDeserialize<List<TransmissionType>>(pref.PreferredTransmissionsJson);

        return new CustomerPreferenceDto
        {
            Id = pref.Id,
            CustomerId = pref.CustomerId,
            MinBudget = pref.MinBudget,
            MaxBudget = pref.MaxBudget,
            PreferredMakes = makes,
            PreferredBodyTypes = bodies,
            PreferredFuelTypes = fuels,
            PreferredTransmissions = transmissions,
            MinYear = pref.MinYear,
            MaxMileage = pref.MaxMileage,
            PreferredCity = pref.PreferredCity,
            UpdatedAt = pref.UpdatedAt
        };
    }

    private static T SafeDeserialize<T>(string json) where T : new()
    {
        if (string.IsNullOrWhiteSpace(json)) return new T();
        try
        {
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }
        catch
        {
            return new T();
        }
    }
}
