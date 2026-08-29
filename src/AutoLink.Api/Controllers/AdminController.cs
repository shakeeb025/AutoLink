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
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IDealerRepository _dealerRepository;
    private readonly IAdminService _adminService;
    private readonly AutoLinkDbContext _context;

    public AdminController(
        IDealerRepository dealerRepository,
        IAdminService adminService,
        AutoLinkDbContext context)
    {
        _dealerRepository = dealerRepository;
        _adminService = adminService;
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<PlatformStatsDto>>> GetStats()
    {
        var stats = await _adminService.GetPlatformStatsAsync();
        return Ok(ApiResponse<PlatformStatsDto>.Ok(stats));
    }

    [HttpGet("pending-sellers")]
    public async Task<ActionResult<ApiResponse<List<DealerProfileDto>>>> GetPendingSellers()
    {
        var pending = await _dealerRepository.GetPendingApprovalDealersAsync();
        return Ok(ApiResponse<List<DealerProfileDto>>.Ok(pending));
    }

    [HttpPost("sellers/approval")]
    public async Task<ActionResult<ApiResponse<string>>> ApproveSeller([FromBody] DealerApprovalDto dto)
    {
        var result = await _dealerRepository.ApproveDealerAsync(dto.DealerId, dto.Approved, dto.Remarks);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("Dealer not found."));

        string action = dto.Approved ? "approved" : "rejected";
        return Ok(ApiResponse<string>.Ok($"Dealer registration successfully {action}."));
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<List<UserInfoDto>>>> GetUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(ApiResponse<List<UserInfoDto>>.Ok(users));
    }

    [HttpPost("moderate-listing")]
    public async Task<ActionResult<ApiResponse<string>>> ModerateListing([FromBody] ModerateListingDto dto)
    {
        var result = await _adminService.ModerateListingAsync(dto.VehicleId, dto.NewStatus, dto.ModerationNotes);
        if (!result)
            return NotFound(ApiResponse<string>.Fail("Listing not found."));

        return Ok(ApiResponse<string>.Ok($"Listing status set to {dto.NewStatus}."));
    }

    [HttpGet("all-listings")]
    public async Task<ActionResult<ApiResponse<List<VehicleDto>>>> GetAllListings([FromQuery] VehicleStatus? status)
    {
        var query = _context.VehicleListings
            .Include(v => v.Dealer)
            .Include(v => v.Images)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(v => v.Status == status.Value);
        }

        var listings = await query.OrderByDescending(v => v.CreatedAt).ToListAsync();

        var dtos = listings.Select(v => new VehicleDto
        {
            Id = v.Id,
            DealerId = v.DealerId,
            DealerName = v.Dealer?.BusinessName ?? "",
            DealerCity = v.Dealer?.City ?? "",
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            Price = v.Price,
            Mileage = v.Mileage,
            BodyType = v.BodyType,
            FuelType = v.FuelType,
            Transmission = v.Transmission,
            Color = v.Color,
            Status = v.Status,
            ViewsCount = v.ViewsCount,
            CreatedAt = v.CreatedAt,
            IsFeatured = v.IsFeatured,
            PrimaryImageUrl = v.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                             ?? v.Images.FirstOrDefault()?.ImageUrl
                             ?? "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800"
        }).ToList();

        return Ok(ApiResponse<List<VehicleDto>>.Ok(dtos));
    }
}
