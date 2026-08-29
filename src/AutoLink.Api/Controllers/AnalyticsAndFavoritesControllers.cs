using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoLink.Core.Interfaces;
using AutoLink.Shared.DTOs;
using AutoLink.Shared.Models;

namespace AutoLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IDealerRepository _dealerRepository;
    private readonly IAdminService _adminService;

    public AnalyticsController(IDealerRepository dealerRepository, IAdminService adminService)
    {
        _dealerRepository = dealerRepository;
        _adminService = adminService;
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("dealer")]
    public async Task<ActionResult<ApiResponse<DealerAnalyticsDto>>> GetDealerAnalytics()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        if (dealer == null) return Forbid();

        var analytics = await _dealerRepository.GetDealerAnalyticsAsync(dealer.Id);
        return Ok(ApiResponse<DealerAnalyticsDto>.Ok(analytics));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult<ApiResponse<PlatformStatsDto>>> GetAdminAnalytics()
    {
        var stats = await _adminService.GetPlatformStatsAsync();
        return Ok(ApiResponse<PlatformStatsDto>.Ok(stats));
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]
public class FavoritesController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;

    public FavoritesController(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> GetMyFavorites()
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerId)) return Unauthorized();

        var favs = await _vehicleRepository.GetCustomerFavoritesAsync(customerId);
        var dtos = favs.Select(f => new VehicleDto
        {
            Id = f.Vehicle.Id,
            DealerId = f.Vehicle.DealerId,
            DealerName = f.Vehicle.Dealer?.BusinessName ?? "",
            DealerCity = f.Vehicle.Dealer?.City ?? "",
            Make = f.Vehicle.Make,
            Model = f.Vehicle.Model,
            Year = f.Vehicle.Year,
            Price = f.Vehicle.Price,
            Mileage = f.Vehicle.Mileage,
            BodyType = f.Vehicle.BodyType,
            FuelType = f.Vehicle.FuelType,
            Transmission = f.Vehicle.Transmission,
            Color = f.Vehicle.Color,
            Status = f.Vehicle.Status,
            ViewsCount = f.Vehicle.ViewsCount,
            CreatedAt = f.Vehicle.CreatedAt,
            IsFeatured = f.Vehicle.IsFeatured,
            IsFavorite = true,
            PrimaryImageUrl = f.Vehicle.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                             ?? f.Vehicle.Images.FirstOrDefault()?.ImageUrl
                             ?? "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800"
        }).ToList();

        return Ok(ApiResponse<List<VehicleDto>>.Ok(dtos));
    }

    [HttpPost("toggle/{vehicleId:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleFavorite(int vehicleId)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerId)) return Unauthorized();

        bool isSaved = await _vehicleRepository.ToggleFavoriteAsync(customerId, vehicleId);
        string msg = isSaved ? "Added to your saved favorites!" : "Removed from your saved favorites.";

        return Ok(ApiResponse<bool>.Ok(isSaved, msg));
    }
}
