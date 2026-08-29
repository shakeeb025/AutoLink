using System.ComponentModel.DataAnnotations;
using AutoLink.Shared.Enums;

namespace AutoLink.Shared.DTOs;

public class VehicleImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class VehicleDto
{
    public int Id { get; set; }
    public int DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public string DealerCity { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public BodyType BodyType { get; set; }
    public FuelType FuelType { get; set; }
    public TransmissionType Transmission { get; set; }
    public string Color { get; set; } = string.Empty;
    public VehicleStatus Status { get; set; }
    public string PrimaryImageUrl { get; set; } = string.Empty;
    public List<VehicleImageDto> Images { get; set; } = new();
    public int ViewsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsFavorite { get; set; }
}

public class VehicleDetailDto : VehicleDto
{
    public string Description { get; set; } = string.Empty;
    public string EngineCapacity { get; set; } = string.Empty;
    public int Horsepower { get; set; }
    public int SeatingCapacity { get; set; }
    public string Vin { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public DealerProfileDto Dealer { get; set; } = new();
}

public class CreateVehicleDto
{
    [Required]
    public string Make { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Year { get; set; } = DateTime.Now.Year;

    [Range(0, 10000000)]
    public decimal Price { get; set; }

    [Range(0, 1000000)]
    public int Mileage { get; set; }

    public BodyType BodyType { get; set; } = BodyType.Sedan;
    public FuelType FuelType { get; set; } = FuelType.Petrol;
    public TransmissionType Transmission { get; set; } = TransmissionType.Automatic;

    [Required]
    public string Color { get; set; } = string.Empty;

    public string EngineCapacity { get; set; } = string.Empty;
    public int Horsepower { get; set; }
    public int SeatingCapacity { get; set; } = 5;
    public string Vin { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
    public bool IsFeatured { get; set; }
}

public class UpdateVehicleDto : CreateVehicleDto
{
    public int Id { get; set; }
    public VehicleStatus Status { get; set; }
}

public class VehicleFilterDto
{
    public string? SearchTerm { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public int? MaxMileage { get; set; }
    public BodyType? BodyType { get; set; }
    public FuelType? FuelType { get; set; }
    public TransmissionType? Transmission { get; set; }
    public string? City { get; set; }
    public VehicleStatus? Status { get; set; } = VehicleStatus.Available;
    public int? DealerId { get; set; }
    public string? SortBy { get; set; } = "DateDesc"; // DateDesc, PriceAsc, PriceDesc, MileageAsc, YearDesc
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class VehicleComparisonDto
{
    public List<VehicleDetailDto> Vehicles { get; set; } = new();
}
