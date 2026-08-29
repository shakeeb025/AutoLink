using System.Security.Claims;
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
public class InquiriesController : ControllerBase
{
    private readonly IInquiryRepository _inquiryRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDealerRepository _dealerRepository;

    public InquiriesController(
        IInquiryRepository inquiryRepository,
        IVehicleRepository vehicleRepository,
        IDealerRepository dealerRepository)
    {
        _inquiryRepository = inquiryRepository;
        _vehicleRepository = vehicleRepository;
        _dealerRepository = dealerRepository;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeadInquiryDto>>> SubmitInquiry([FromBody] CreateInquiryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<LeadInquiryDto>.Fail("Invalid inquiry submission"));

        var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
        if (vehicle == null)
            return NotFound(ApiResponse<LeadInquiryDto>.Fail("Vehicle not found."));

        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var inquiry = new LeadInquiry
        {
            VehicleId = dto.VehicleId,
            DealerId = vehicle.DealerId,
            CustomerId = customerId,
            CustomerName = dto.CustomerName.Trim(),
            CustomerEmail = dto.CustomerEmail.Trim(),
            CustomerPhone = dto.CustomerPhone.Trim(),
            Message = dto.Message.Trim(),
            Status = InquiryStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _inquiryRepository.AddAsync(inquiry);

        return Ok(ApiResponse<LeadInquiryDto>.Ok(new LeadInquiryDto
        {
            Id = created.Id,
            VehicleId = created.VehicleId,
            DealerId = created.DealerId,
            CustomerName = created.CustomerName,
            CustomerEmail = created.CustomerEmail,
            Message = created.Message,
            Status = created.Status,
            CreatedAt = created.CreatedAt
        }, "Inquiry sent to dealership! You will be contacted shortly."));
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("my-inquiries")]
    public async Task<ActionResult<ApiResponse<List<LeadInquiryDto>>>> GetMyInquiries()
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerId)) return Unauthorized();

        var inquiries = await _inquiryRepository.GetCustomerInquiriesAsync(customerId);
        return Ok(ApiResponse<List<LeadInquiryDto>>.Ok(inquiries));
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("dealer-inquiries")]
    public async Task<ActionResult<ApiResponse<List<LeadInquiryDto>>>> GetDealerInquiries()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        if (dealer == null) return Forbid();

        var inquiries = await _inquiryRepository.GetDealerInquiriesAsync(dealer.Id);
        return Ok(ApiResponse<List<LeadInquiryDto>>.Ok(inquiries));
    }

    [Authorize(Roles = "Seller")]
    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateInquiryStatus([FromBody] UpdateInquiryStatusDto dto)
    {
        var inquiry = await _inquiryRepository.GetByIdAsync(dto.InquiryId);
        if (inquiry == null)
            return NotFound(ApiResponse<string>.Fail("Inquiry not found"));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var dealer = await _dealerRepository.GetByUserIdAsync(userId!);
        if (dealer == null || inquiry.DealerId != dealer.Id)
            return Forbid();

        await _inquiryRepository.UpdateStatusAsync(dto.InquiryId, dto.Status, dto.DealerResponse);
        return Ok(ApiResponse<string>.Ok("Inquiry updated successfully."));
    }
}
