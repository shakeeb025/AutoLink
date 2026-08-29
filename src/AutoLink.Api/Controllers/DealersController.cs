using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoLink.Core.Interfaces;
using AutoLink.Infrastructure.Data;
using AutoLink.Shared.DTOs;
using AutoLink.Shared.Enums;
using AutoLink.Shared.Models;

namespace AutoLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DealersController : ControllerBase
{
    private readonly IDealerRepository _dealerRepository;
    private readonly AutoLinkDbContext _context;

    public DealersController(IDealerRepository dealerRepository, AutoLinkDbContext context)
    {
        _dealerRepository = dealerRepository;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DealerProfileDto>>>> GetDealers([FromQuery] DealerApprovalStatus? status = DealerApprovalStatus.Approved)
    {
        var dealers = await _dealerRepository.GetAllDealersAsync(status);
        return Ok(ApiResponse<List<DealerProfileDto>>.Ok(dealers));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<DealerProfileDto>>> GetDealerById(int id)
    {
        var dealer = await _dealerRepository.GetByIdAsync(id);
        if (dealer == null)
            return NotFound(ApiResponse<DealerProfileDto>.Fail("Dealer not found"));

        var dto = new DealerProfileDto
        {
            Id = dealer.Id,
            UserId = dealer.UserId,
            BusinessName = dealer.BusinessName,
            RegistrationNumber = dealer.RegistrationNumber,
            ContactPersonName = dealer.ContactPersonName,
            Email = dealer.User?.Email ?? "",
            PhoneNumber = dealer.PhoneNumber,
            Address = dealer.Address,
            City = dealer.City,
            LogoUrl = dealer.LogoUrl,
            Description = dealer.Description,
            ApprovalStatus = dealer.ApprovalStatus,
            SubscriptionTier = dealer.SubscriptionTier,
            SubscriptionExpiry = dealer.SubscriptionExpiry,
            ActiveListingsCount = dealer.Listings.Count(v => v.Status == VehicleStatus.Available),
            MaxListingLimit = dealer.MaxListingLimit,
            Rating = dealer.Rating,
            CreatedAt = dealer.CreatedAt
        };

        return Ok(ApiResponse<DealerProfileDto>.Ok(dto));
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("my-profile")]
    public async Task<ActionResult<ApiResponse<DealerProfileDto>>> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        if (dealer == null) return NotFound(ApiResponse<DealerProfileDto>.Fail("Dealer profile not found"));

        var dto = new DealerProfileDto
        {
            Id = dealer.Id,
            UserId = dealer.UserId,
            BusinessName = dealer.BusinessName,
            RegistrationNumber = dealer.RegistrationNumber,
            ContactPersonName = dealer.ContactPersonName,
            Email = dealer.User?.Email ?? "",
            PhoneNumber = dealer.PhoneNumber,
            Address = dealer.Address,
            City = dealer.City,
            LogoUrl = dealer.LogoUrl,
            Description = dealer.Description,
            ApprovalStatus = dealer.ApprovalStatus,
            SubscriptionTier = dealer.SubscriptionTier,
            SubscriptionExpiry = dealer.SubscriptionExpiry,
            ActiveListingsCount = dealer.Listings.Count(v => v.Status == VehicleStatus.Available),
            MaxListingLimit = dealer.MaxListingLimit,
            Rating = dealer.Rating,
            CreatedAt = dealer.CreatedAt
        };

        return Ok(ApiResponse<DealerProfileDto>.Ok(dto));
    }

    [Authorize(Roles = "Seller")]
    [HttpPut("my-profile")]
    public async Task<ActionResult<ApiResponse<DealerProfileDto>>> UpdateMyProfile([FromBody] DealerProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        if (dealer == null) return NotFound(ApiResponse<DealerProfileDto>.Fail("Dealer profile not found"));

        dealer.BusinessName = dto.BusinessName.Trim();
        dealer.ContactPersonName = dto.ContactPersonName.Trim();
        dealer.PhoneNumber = dto.PhoneNumber.Trim();
        dealer.Address = dto.Address.Trim();
        dealer.City = dto.City.Trim();
        dealer.LogoUrl = dto.LogoUrl;
        dealer.Description = dto.Description;

        await _dealerRepository.UpdateAsync(dealer);

        dto.Id = dealer.Id;
        dto.UserId = dealer.UserId;
        dto.ApprovalStatus = dealer.ApprovalStatus;
        dto.SubscriptionTier = dealer.SubscriptionTier;

        return Ok(ApiResponse<DealerProfileDto>.Ok(dto, "Dealer profile updated successfully!"));
    }

    [HttpGet("subscription-plans")]
    public async Task<ActionResult<ApiResponse<List<SubscriptionPlanDto>>>> GetSubscriptionPlans()
    {
        var plans = await _context.SubscriptionPlans.ToListAsync();
        var dtos = plans.Select(p => new SubscriptionPlanDto
        {
            Tier = p.Tier,
            Name = p.Name,
            MonthlyPrice = p.MonthlyPrice,
            MaxListings = p.MaxListings,
            PrioritySearch = p.PrioritySearch,
            FeaturedBadges = p.FeaturedBadges,
            AdvancedAnalytics = p.AdvancedAnalytics,
            StaffAccounts = p.StaffAccounts,
            Features = System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.FeaturesJson) ?? new()
        }).ToList();

        return Ok(ApiResponse<List<SubscriptionPlanDto>>.Ok(dtos));
    }

    [Authorize(Roles = "Seller")]
    [HttpPost("upgrade-tier")]
    public async Task<ActionResult<ApiResponse<string>>> UpgradeTier([FromBody] SubscriptionTier newTier)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        if (dealer == null) return NotFound(ApiResponse<string>.Fail("Dealer not found"));

        dealer.SubscriptionTier = newTier;
        dealer.MaxListingLimit = newTier switch
        {
            SubscriptionTier.Standard => 25,
            SubscriptionTier.Premium => 100,
            _ => 5
        };
        dealer.SubscriptionExpiry = DateTime.UtcNow.AddYears(1);

        await _dealerRepository.UpdateAsync(dealer);
        return Ok(ApiResponse<string>.Ok($"Successfully updated plan to {newTier} tier!"));
    }
}
