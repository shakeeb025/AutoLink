using AutoLink.Core.Entities;
using AutoLink.Core.Interfaces;
using AutoLink.Core.Services;
using AutoLink.Shared.DTOs;
using AutoLink.Shared.Enums;
using Moq;
using Xunit;

namespace AutoLink.Tests;

public class RecommendationEngineTests
{
    private readonly Mock<IVehicleRepository> _mockVehicleRepo;
    private readonly Mock<ICustomerPreferenceRepository> _mockPrefRepo;
    private readonly RecommendationService _service;

    public RecommendationEngineTests()
    {
        _mockVehicleRepo = new Mock<IVehicleRepository>();
        _mockPrefRepo = new Mock<ICustomerPreferenceRepository>();
        _service = new RecommendationService(_mockVehicleRepo.Object, _mockPrefRepo.Object);
    }

    [Fact]
    public void CalculateMatchScore_ExactMatch_YieldsNear100Percent()
    {
        // Arrange
        var vehicle = new VehicleListing
        {
            Id = 1,
            Make = "Tesla",
            Model = "Model S",
            Year = 2023,
            Price = 60000m,
            Mileage = 15000,
            BodyType = BodyType.Sedan,
            FuelType = FuelType.Electric,
            Transmission = TransmissionType.Automatic,
            Color = "Midnight Silver",
            Status = VehicleStatus.Available
        };

        var pref = new CustomerPreferenceDto
        {
            MinBudget = 50000,
            MaxBudget = 70000,
            PreferredMakes = new() { "Tesla" },
            PreferredBodyTypes = new() { BodyType.Sedan },
            PreferredFuelTypes = new() { FuelType.Electric },
            PreferredTransmissions = new() { TransmissionType.Automatic },
            MinYear = 2022,
            MaxMileage = 25000
        };

        // Act
        var score = _service.CalculateMatchScore(vehicle, pref);
        double total = score.BudgetScore + score.BodyTypeScore + score.FuelTransmissionScore + score.YearScore + score.MileageScore;

        // Assert
        Assert.Equal(30.0, score.BudgetScore);              // 30%
        Assert.Equal(20.0, score.BodyTypeScore);            // 20%
        Assert.Equal(20.0, score.FuelTransmissionScore);    // 10% + 10%
        Assert.Equal(15.0, score.YearScore);                // 15%
        Assert.Equal(15.0, score.MileageScore);             // 15%
        Assert.Equal(100.0, total);
        Assert.Contains("Tesla", score.MatchReasons[0]);
    }

    [Fact]
    public void CalculateMatchScore_MismatchedBodyAndOverBudget_AppliesDecayCorrectly()
    {
        // Arrange
        var vehicle = new VehicleListing
        {
            Id = 2,
            Make = "Ford",
            Model = "Mustang Mach-E",
            Year = 2021,
            Price = 90000m, // Over max budget of 70,000
            Mileage = 50000, // Over max mileage of 30,000
            BodyType = BodyType.SUV, // Doesn't match Sedan
            FuelType = FuelType.Electric,
            Transmission = TransmissionType.Automatic,
            Status = VehicleStatus.Available
        };

        var pref = new CustomerPreferenceDto
        {
            MinBudget = 40000,
            MaxBudget = 70000,
            PreferredBodyTypes = new() { BodyType.Sedan },
            PreferredFuelTypes = new() { FuelType.Electric },
            PreferredTransmissions = new() { TransmissionType.Automatic },
            MinYear = 2023, // Vehicle is 2021 (2 years older)
            MaxMileage = 30000
        };

        // Act
        var score = _service.CalculateMatchScore(vehicle, pref);

        // Assert
        Assert.True(score.BudgetScore < 30.0);
        Assert.Equal(0.0, score.BodyTypeScore); // SUV != Sedan
        Assert.Equal(20.0, score.FuelTransmissionScore); // Electric + Auto match
        Assert.True(score.YearScore < 15.0); // 2021 < 2023
        Assert.True(score.MileageScore < 15.0); // 50k > 30k
    }

    [Fact]
    public async Task GetRecommendationsWithCustomPreferencesAsync_ReturnsDescendingRankedList()
    {
        // Arrange
        var v1 = new VehicleListing
        {
            Id = 1,
            Make = "BMW",
            Model = "M3",
            Year = 2023,
            Price = 80000m,
            Mileage = 10000,
            BodyType = BodyType.Sedan,
            FuelType = FuelType.Petrol,
            Transmission = TransmissionType.Automatic,
            Status = VehicleStatus.Available,
            Images = new List<VehicleImage> { new() { ImageUrl = "http://test.com/1.jpg", IsPrimary = true } }
        };

        var v2 = new VehicleListing
        {
            Id = 2,
            Make = "Toyota",
            Model = "Yaris",
            Year = 2012,
            Price = 8000m,
            Mileage = 140000,
            BodyType = BodyType.Hatchback,
            FuelType = FuelType.Diesel,
            Transmission = TransmissionType.Manual,
            Status = VehicleStatus.Available,
            Images = new List<VehicleImage> { new() { ImageUrl = "http://test.com/2.jpg", IsPrimary = true } }
        };

        _mockVehicleRepo.Setup(r => r.GetAllAvailableListingsAsync())
            .ReturnsAsync(new List<VehicleListing> { v2, v1 });

        var pref = new CustomerPreferenceDto
        {
            MinBudget = 70000,
            MaxBudget = 90000,
            PreferredBodyTypes = new() { BodyType.Sedan },
            PreferredFuelTypes = new() { FuelType.Petrol },
            PreferredTransmissions = new() { TransmissionType.Automatic },
            MinYear = 2022,
            MaxMileage = 20000
        };

        // Act
        var results = (await _service.GetRecommendationsWithCustomPreferencesAsync(pref)).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal(1, results[0].Vehicle.Id); // BMW M3 should be top rank
        Assert.True(results[0].MatchPercentage > results[1].MatchPercentage);
    }
}
