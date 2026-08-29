namespace AutoLink.Shared.Enums;

public enum UserRole
{
    Customer,
    Seller,
    Admin
}

public enum VehicleStatus
{
    Available,
    Reserved,
    Sold,
    UnderReview,
    Suspended
}

public enum FuelType
{
    Petrol,
    Diesel,
    Electric,
    Hybrid,
    PlugInHybrid,
    CNG
}

public enum TransmissionType
{
    Automatic,
    Manual,
    DualClutch,
    CVT
}

public enum BodyType
{
    Sedan,
    SUV,
    Hatchback,
    Coupe,
    Convertible,
    Wagon,
    Pickup,
    Van,
    Crossover
}

public enum DealerApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    Suspended
}

public enum SubscriptionTier
{
    Free,
    Standard,
    Premium
}

public enum BookingStatus
{
    Requested,
    Approved,
    Rejected,
    Rescheduled,
    Completed,
    Cancelled
}

public enum InquiryStatus
{
    New,
    Contacted,
    InDiscussion,
    Closed
}
