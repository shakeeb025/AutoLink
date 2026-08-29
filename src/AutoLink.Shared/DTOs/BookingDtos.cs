using System.ComponentModel.DataAnnotations;
using AutoLink.Shared.Enums;

namespace AutoLink.Shared.DTOs;

public class CreateTestDriveDto
{
    [Required]
    public int VehicleId { get; set; }

    [Required]
    public DateTime ScheduledDate { get; set; }

    [Required]
    public string PreferredTimeSlot { get; set; } = "10:00 AM - 11:00 AM";

    public string Notes { get; set; } = string.Empty;
    public string CustomerContactNumber { get; set; } = string.Empty;
}

public class TestDriveDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleTitle { get; set; } = string.Empty;
    public string VehicleImageUrl { get; set; } = string.Empty;
    public int DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public string DealerAddress { get; set; } = string.Empty;
    public string DealerPhone { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string PreferredTimeSlot { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public string? DealerNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateBookingStatusDto
{
    public int BookingId { get; set; }
    public BookingStatus Status { get; set; }
    public string? DealerNotes { get; set; }
}

public class CreateInquiryDto
{
    [Required]
    public int VehicleId { get; set; }

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;
}

public class LeadInquiryDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleTitle { get; set; } = string.Empty;
    public string VehicleImageUrl { get; set; } = string.Empty;
    public decimal VehiclePrice { get; set; }
    public int DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public InquiryStatus Status { get; set; }
    public string? DealerResponse { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateInquiryStatusDto
{
    public int InquiryId { get; set; }
    public InquiryStatus Status { get; set; }
    public string? DealerResponse { get; set; }
}
