using AutoLink.Shared.Enums;

namespace AutoLink.Shared.DTOs;

public class CustomerPreferenceDto
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal? MinBudget { get; set; }
    public decimal? MaxBudget { get; set; }
    public List<string> PreferredMakes { get; set; } = new();
    public List<BodyType> PreferredBodyTypes { get; set; } = new();
    public List<FuelType> PreferredFuelTypes { get; set; } = new();
    public List<TransmissionType> PreferredTransmissions { get; set; } = new();
    public int? MinYear { get; set; }
    public int? MaxMileage { get; set; }
    public string? PreferredCity { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MatchBreakdownDto
{
    public double BudgetScore { get; set; }          // Max 30%
    public double BodyTypeScore { get; set; }        // Max 20%
    public double FuelTransmissionScore { get; set; } // Max 20% (10% fuel + 10% trans)
    public double YearScore { get; set; }            // Max 15%
    public double MileageScore { get; set; }         // Max 15%
    public List<string> MatchReasons { get; set; } = new();
}

public class VehicleMatchDto
{
    public VehicleDto Vehicle { get; set; } = new();
    public double MatchPercentage { get; set; }
    public MatchBreakdownDto Breakdown { get; set; } = new();
}
