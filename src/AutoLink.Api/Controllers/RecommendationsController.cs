using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoLink.Core.Interfaces;
using AutoLink.Shared.DTOs;
using AutoLink.Shared.Models;

namespace AutoLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly ICustomerPreferenceRepository _preferenceRepository;

    public RecommendationsController(
        IRecommendationService recommendationService,
        ICustomerPreferenceRepository preferenceRepository)
    {
        _recommendationService = recommendationService;
        _preferenceRepository = preferenceRepository;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<VehicleMatchDto>>>> GetRecommendations()
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(customerId))
        {
            // For guest visitors, evaluate with generic default preferences
            var defaultPrefs = new CustomerPreferenceDto
            {
                MinBudget = 25000,
                MaxBudget = 90000,
                MinYear = 2021,
                MaxMileage = 45000
            };
            var guestMatches = await _recommendationService.GetRecommendationsWithCustomPreferencesAsync(defaultPrefs);
            return Ok(ApiResponse<IEnumerable<VehicleMatchDto>>.Ok(guestMatches));
        }

        var matches = await _recommendationService.GetRecommendationsAsync(customerId);
        return Ok(ApiResponse<IEnumerable<VehicleMatchDto>>.Ok(matches));
    }

    [HttpPost("evaluate")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VehicleMatchDto>>>> EvaluateCustomPreferences([FromBody] CustomerPreferenceDto preferences)
    {
        var matches = await _recommendationService.GetRecommendationsWithCustomPreferencesAsync(preferences);
        return Ok(ApiResponse<IEnumerable<VehicleMatchDto>>.Ok(matches));
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("preferences")]
    public async Task<ActionResult<ApiResponse<CustomerPreferenceDto>>> GetPreferences()
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerId)) return Unauthorized();

        var pref = await _preferenceRepository.GetByCustomerIdAsync(customerId);
        if (pref == null)
        {
            return Ok(ApiResponse<CustomerPreferenceDto>.Ok(new CustomerPreferenceDto { CustomerId = customerId }));
        }

        var dto = new CustomerPreferenceDto
        {
            Id = pref.Id,
            CustomerId = pref.CustomerId,
            MinBudget = pref.MinBudget,
            MaxBudget = pref.MaxBudget,
            MinYear = pref.MinYear,
            MaxMileage = pref.MaxMileage,
            PreferredCity = pref.PreferredCity,
            UpdatedAt = pref.UpdatedAt
        };

        return Ok(ApiResponse<CustomerPreferenceDto>.Ok(dto));
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("preferences")]
    public async Task<ActionResult<ApiResponse<CustomerPreferenceDto>>> SavePreferences([FromBody] CustomerPreferenceDto dto)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerId)) return Unauthorized();

        var saved = await _preferenceRepository.SavePreferenceAsync(customerId, dto);
        dto.Id = saved.Id;
        dto.CustomerId = customerId;
        dto.UpdatedAt = saved.UpdatedAt;

        return Ok(ApiResponse<CustomerPreferenceDto>.Ok(dto, "Vehicle preferences saved successfully!"));
    }
}
