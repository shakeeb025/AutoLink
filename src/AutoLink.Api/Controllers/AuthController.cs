using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoLink.Core.Entities;
using AutoLink.Core.Interfaces;
using AutoLink.Shared.DTOs;
using AutoLink.Shared.Enums;
using AutoLink.Shared.Models;

namespace AutoLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IDealerRepository _dealerRepository;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IDealerRepository dealerRepository)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dealerRepository = dealerRepository;
    }

    [HttpPost("register-customer")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RegisterCustomer([FromBody] RegisterCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("An account with this email address already exists."));

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            City = dto.City,
            Role = UserRole.Customer,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Registration failed", result.Errors.Select(e => e.Description).ToList()));

        await _userManager.AddToRoleAsync(user, UserRole.Customer.ToString());
        var authResponse = await _tokenService.GenerateTokenAsync(user);

        return Ok(ApiResponse<AuthResponseDto>.Ok(authResponse, "Customer account registered successfully!"));
    }

    [HttpPost("register-seller")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RegisterSeller([FromBody] RegisterSellerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Validation failed", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("An account with this email address already exists."));

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.ContactPersonName,
            PhoneNumber = dto.PhoneNumber,
            City = dto.City,
            Role = UserRole.Seller,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Seller user registration failed", result.Errors.Select(e => e.Description).ToList()));

        await _userManager.AddToRoleAsync(user, UserRole.Seller.ToString());

        int limit = dto.RequestedTier switch
        {
            SubscriptionTier.Standard => 25,
            SubscriptionTier.Premium => 100,
            _ => 5
        };

        var dealer = new DealerProfile
        {
            UserId = user.Id,
            BusinessName = dto.BusinessName,
            RegistrationNumber = dto.RegistrationNumber,
            ContactPersonName = dto.ContactPersonName,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            City = dto.City,
            ApprovalStatus = DealerApprovalStatus.Pending, // Awaiting administrator approval
            SubscriptionTier = dto.RequestedTier,
            MaxListingLimit = limit,
            CreatedAt = DateTime.UtcNow
        };

        await _dealerRepository.AddAsync(dealer);
        var authResponse = await _tokenService.GenerateTokenAsync(user, dealer);

        return Ok(ApiResponse<AuthResponseDto>.Ok(authResponse, "Dealership registered! Your account is submitted for Administrator approval."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Invalid credentials submitted."));

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid email or password."));

        if (!user.IsActive)
            return StatusCode(403, ApiResponse<AuthResponseDto>.Fail("Your account has been deactivated. Please contact support."));

        DealerProfile? dealer = null;
        if (user.Role == UserRole.Seller)
        {
            dealer = await _dealerRepository.GetByUserIdAsync(user.Id);
        }

        var authResponse = await _tokenService.GenerateTokenAsync(user, dealer);
        return Ok(ApiResponse<AuthResponseDto>.Ok(authResponse, "Login successful!"));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserInfoDto>>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<UserInfoDto>.Fail("Unauthorized"));

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(ApiResponse<UserInfoDto>.Fail("User not found"));

        DealerProfile? dealer = null;
        if (user.Role == UserRole.Seller)
        {
            dealer = await _dealerRepository.GetByUserIdAsync(user.Id);
        }

        var userInfo = new UserInfoDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            Role = user.Role.ToString(),
            DealerId = dealer?.Id,
            DealerName = dealer?.BusinessName,
            DealerStatus = dealer?.ApprovalStatus,
            SubscriptionTier = dealer?.SubscriptionTier
        };

        return Ok(ApiResponse<UserInfoDto>.Ok(userInfo));
    }
}
