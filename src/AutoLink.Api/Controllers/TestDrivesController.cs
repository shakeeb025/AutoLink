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
public class TestDrivesController : ControllerBase
{
    private readonly ITestDriveRepository _testDriveRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDealerRepository _dealerRepository;

    public TestDrivesController(
        ITestDriveRepository testDriveRepository,
        IVehicleRepository vehicleRepository,
        IDealerRepository dealerRepository)
    {
        _testDriveRepository = testDriveRepository;
        _vehicleRepository = vehicleRepository;
        _dealerRepository = dealerRepository;
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TestDriveDto>>> CreateBooking([FromBody] CreateTestDriveDto dto)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerId)) return Unauthorized();

        var vehicle = await _vehicleRepository.GetByIdAsync(dto.VehicleId);
        if (vehicle == null)
            return NotFound(ApiResponse<TestDriveDto>.Fail("Vehicle not found."));

        if (vehicle.Status != VehicleStatus.Available)
            return BadRequest(ApiResponse<TestDriveDto>.Fail("This vehicle is currently not available for test drives."));

        var booking = new TestDriveBooking
        {
            VehicleId = dto.VehicleId,
            DealerId = vehicle.DealerId,
            CustomerId = customerId,
            ScheduledDate = dto.ScheduledDate,
            PreferredTimeSlot = dto.PreferredTimeSlot,
            Notes = dto.Notes,
            CustomerContactNumber = dto.CustomerContactNumber,
            Status = BookingStatus.Requested,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _testDriveRepository.AddAsync(booking);
        var result = await _testDriveRepository.GetByIdAsync(created.Id);

        return Ok(ApiResponse<TestDriveDto>.Ok(new TestDriveDto
        {
            Id = created.Id,
            VehicleId = created.VehicleId,
            DealerId = created.DealerId,
            ScheduledDate = created.ScheduledDate,
            PreferredTimeSlot = created.PreferredTimeSlot,
            Notes = created.Notes,
            Status = created.Status,
            CreatedAt = created.CreatedAt
        }, "Test drive appointment requested successfully!"));
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("my-bookings")]
    public async Task<ActionResult<ApiResponse<List<TestDriveDto>>>> GetMyBookings()
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(customerId)) return Unauthorized();

        var bookings = await _testDriveRepository.GetCustomerBookingsAsync(customerId);
        return Ok(ApiResponse<List<TestDriveDto>>.Ok(bookings));
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("dealer-bookings")]
    public async Task<ActionResult<ApiResponse<List<TestDriveDto>>>> GetDealerBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var dealer = await _dealerRepository.GetByUserIdAsync(userId);
        if (dealer == null) return Forbid();

        var bookings = await _testDriveRepository.GetDealerBookingsAsync(dealer.Id);
        return Ok(ApiResponse<List<TestDriveDto>>.Ok(bookings));
    }

    [Authorize(Roles = "Seller,Admin")]
    [HttpPut("status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateBookingStatus([FromBody] UpdateBookingStatusDto dto)
    {
        var booking = await _testDriveRepository.GetByIdAsync(dto.BookingId);
        if (booking == null)
            return NotFound(ApiResponse<string>.Fail("Booking not found"));

        if (User.IsInRole("Seller"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var dealer = await _dealerRepository.GetByUserIdAsync(userId!);
            if (dealer == null || booking.DealerId != dealer.Id)
                return Forbid();
        }

        await _testDriveRepository.UpdateStatusAsync(dto.BookingId, dto.Status, dto.DealerNotes);
        return Ok(ApiResponse<string>.Ok($"Test drive status updated to {dto.Status}."));
    }
}
