using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoLink.Core.Entities;
using AutoLink.Core.Interfaces;
using AutoLink.Shared.DTOs;
using AutoLink.Shared.Enums;
using AutoLink.Shared.Models;

namespace AutoLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDealerRepository _dealerRepository;

    public VehiclesController(IVehicleRepository vehicleRepository, IDealerRepository dealerRepository)
    {
        _vehicleRepository = vehicleRepository;
        _dealerRepository = dealerRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<VehicleDto>>>> GetVehicles([FromQuery] VehicleFilterDto filter)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _vehicleRepository.GetVehiclesAsync(filter, currentUserId);
        return Ok(ApiResponse<PagedResult<VehicleDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<VehicleDetailDto>>> GetVehicleById(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var vehicle = await _vehicleRepository.GetDetailByIdAsync(id, currentUserId);
        if (vehicle == null)
            return NotFound(ApiResponse<VehicleDetailDto>.Fail($"Vehicle with ID {id} not found."));

        // Fire and forget view increment
        _ = _vehicleRepository.IncrementViewsAsync(id);

        return Ok(ApiResponse<VehicleDetailDto>.Ok(vehicle));
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("my-inventory")]
    public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> GetMyInventory([FromQuery] VehicleStatus? status)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        if (dealer == null) return Forbid();

        var listings = await _vehicleRepository.GetSellerListingsAsync(dealer.Id, status);
        return Ok(ApiResponse<List<VehicleDto>>.Ok(listings));
    }

    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> CreateVehicle([FromBody] CreateVehicleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<VehicleDto>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        if (dealer == null) return Forbid();

        if (dealer.ApprovalStatus != DealerApprovalStatus.Approved)
            return BadRequest(ApiResponse<VehicleDto>.Fail("Your dealership profile is currently pending administrator verification. Listings can be created once approved."));

        // Check subscription listing quota
        int activeCount = dealer.Listings.Count(v => v.Status == VehicleStatus.Available);
        if (activeCount >= dealer.MaxListingLimit)
            return BadRequest(ApiResponse<VehicleDto>.Fail($"You have reached the maximum listing limit ({dealer.MaxListingLimit}) for your current SaaS plan ({dealer.SubscriptionTier}). Please upgrade to add more inventory."));

        var vehicle = new VehicleListing
        {
            DealerId = dealer.Id,
            Make = dto.Make.Trim(),
            Model = dto.Model.Trim(),
            Year = dto.Year,
            Price = dto.Price,
            Mileage = dto.Mileage,
            BodyType = dto.BodyType,
            FuelType = dto.FuelType,
            Transmission = dto.Transmission,
            Color = dto.Color.Trim(),
            EngineCapacity = dto.EngineCapacity,
            Horsepower = dto.Horsepower,
            SeatingCapacity = dto.SeatingCapacity,
            Vin = dto.Vin,
            Description = dto.Description,
            FeaturesJson = JsonSerializer.Serialize(dto.Features ?? new()),
            Status = VehicleStatus.Available,
            IsFeatured = dto.IsFeatured,
            CreatedAt = DateTime.UtcNow
        };

        // Add images
        if (dto.ImageUrls != null && dto.ImageUrls.Count > 0)
        {
            int order = 1;
            foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                vehicle.Images.Add(new VehicleImage
                {
                    ImageUrl = url.Trim(),
                    IsPrimary = (order == 1),
                    DisplayOrder = order++
                });
            }
        }
        else
        {
            vehicle.Images.Add(new VehicleImage
            {
                ImageUrl = "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800",
                IsPrimary = true,
                DisplayOrder = 1
            });
        }

        var created = await _vehicleRepository.AddAsync(vehicle);

        var resultDto = new VehicleDto
        {
            Id = created.Id,
            DealerId = dealer.Id,
            DealerName = dealer.BusinessName,
            DealerCity = dealer.City,
            Make = created.Make,
            Model = created.Model,
            Year = created.Year,
            Price = created.Price,
            Mileage = created.Mileage,
            BodyType = created.BodyType,
            FuelType = created.FuelType,
            Transmission = created.Transmission,
            Color = created.Color,
            Status = created.Status,
            PrimaryImageUrl = created.Images.FirstOrDefault()?.ImageUrl ?? "",
            CreatedAt = created.CreatedAt
        };

        return CreatedAtAction(nameof(GetVehicleById), new { id = created.Id }, ApiResponse<VehicleDto>.Ok(resultDto, "Vehicle listing created successfully!"));
    }

    [Authorize(Roles = "Seller")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateVehicle(int id, [FromBody] UpdateVehicleDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        if (dealer == null) return Forbid();

        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) return NotFound(ApiResponse<string>.Fail("Listing not found"));

        if (vehicle.DealerId != dealer.Id)
            return Forbid();

        vehicle.Make = dto.Make.Trim();
        vehicle.Model = dto.Model.Trim();
        vehicle.Year = dto.Year;
        vehicle.Price = dto.Price;
        vehicle.Mileage = dto.Mileage;
        vehicle.BodyType = dto.BodyType;
        vehicle.FuelType = dto.FuelType;
        vehicle.Transmission = dto.Transmission;
        vehicle.Color = dto.Color.Trim();
        vehicle.EngineCapacity = dto.EngineCapacity;
        vehicle.Horsepower = dto.Horsepower;
        vehicle.SeatingCapacity = dto.SeatingCapacity;
        vehicle.Vin = dto.Vin;
        vehicle.Description = dto.Description;
        vehicle.FeaturesJson = JsonSerializer.Serialize(dto.Features ?? new());
        vehicle.Status = dto.Status;
        vehicle.IsFeatured = dto.IsFeatured;
        vehicle.UpdatedAt = DateTime.UtcNow;

        // Update images if provided
        if (dto.ImageUrls != null && dto.ImageUrls.Count > 0)
        {
            vehicle.Images.Clear();
            int order = 1;
            foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                vehicle.Images.Add(new VehicleImage
                {
                    ImageUrl = url.Trim(),
                    IsPrimary = (order == 1),
                    DisplayOrder = order++
                });
            }
        }

        await _vehicleRepository.UpdateAsync(vehicle);
        return Ok(ApiResponse<string>.Ok("Vehicle listing updated successfully!"));
    }

    [Authorize(Roles = "Seller")]
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(int id, [FromBody] VehicleStatus newStatus)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        if (dealer == null) return Forbid();

        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) return NotFound(ApiResponse<string>.Fail("Listing not found"));

        if (vehicle.DealerId != dealer.Id)
            return Forbid();

        vehicle.Status = newStatus;
        vehicle.UpdatedAt = DateTime.UtcNow;
        await _vehicleRepository.UpdateAsync(vehicle);

        return Ok(ApiResponse<string>.Ok($"Status changed to {newStatus}"));
    }

    [Authorize(Roles = "Seller,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteVehicle(int id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) return NotFound(ApiResponse<string>.Fail("Vehicle not found"));

        if (User.IsInRole("Seller"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dealer = await _dealerRepository.GetByUserIdAsync(userId!);
            if (dealer == null || vehicle.DealerId != dealer.Id)
                return Forbid();
        }

        await _vehicleRepository.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Vehicle listing deleted successfully."));
    }
}
