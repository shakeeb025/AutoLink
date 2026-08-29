using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using AutoLink.Core.Entities;
using AutoLink.Shared.Enums;

namespace AutoLink.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        AutoLinkDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // 1. Seed Roles
        string[] roles = { "Admin", "Seller", "Customer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Subscription Plans
        if (!context.SubscriptionPlans.Any())
        {
            context.SubscriptionPlans.AddRange(
                new SubscriptionPlan
                {
                    Tier = SubscriptionTier.Free,
                    Name = "Starter Dealer",
                    MonthlyPrice = 0m,
                    MaxListings = 5,
                    PrioritySearch = false,
                    FeaturedBadges = false,
                    AdvancedAnalytics = false,
                    StaffAccounts = false,
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "Up to 5 active vehicle listings", "Standard customer lead inbox", "Basic test drive booking manager", "Marketplace listing visibility" })
                },
                new SubscriptionPlan
                {
                    Tier = SubscriptionTier.Standard,
                    Name = "Professional Dealer",
                    MonthlyPrice = 149.00m,
                    MaxListings = 25,
                    PrioritySearch = true,
                    FeaturedBadges = true,
                    AdvancedAnalytics = true,
                    StaffAccounts = false,
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "Up to 25 active vehicle listings", "Priority ranking in search & match", "Featured badge on inventory", "Full sales & lead conversion analytics", "Direct customer messaging" })
                },
                new SubscriptionPlan
                {
                    Tier = SubscriptionTier.Premium,
                    Name = "Enterprise Dealer Hub",
                    MonthlyPrice = 399.00m,
                    MaxListings = 100,
                    PrioritySearch = true,
                    FeaturedBadges = true,
                    AdvancedAnalytics = true,
                    StaffAccounts = true,
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "Up to 100 active vehicle listings", "Top-tier AI match priority", "Multiple staff sub-accounts", "Multi-branch inventory synchronization", "Dedicated dealer success manager", "Exportable CSV & PDF financial reports" })
                }
            );
            await context.SaveChangesAsync();
        }

        // 3. Seed Admin User
        var adminEmail = "admin@autolink.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "AutoLink Chief Administrator",
                Role = UserRole.Admin,
                City = "San Francisco",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // 4. Seed Dealerships (Sellers)
        var dealer1User = await EnsureUserAsync(userManager, "prime@motors.com", "Prime Motors Manager", UserRole.Seller, "Seller@123", "Los Angeles");
        var dealer2User = await EnsureUserAsync(userManager, "apex@auto.com", "Apex Prestige Director", UserRole.Seller, "Seller@123", "San Francisco");
        var dealer3User = await EnsureUserAsync(userManager, "metro@dealers.com", "Metro Auto Hub Sales", UserRole.Seller, "Seller@123", "San Diego");
        var dealer4User = await EnsureUserAsync(userManager, "pacific@coast.com", "Pacific Coast Motors", UserRole.Seller, "Seller@123", "Seattle");

        DealerProfile? d1 = null, d2 = null, d3 = null, d4 = null;

        if (!context.DealerProfiles.Any())
        {
            d1 = new DealerProfile
            {
                UserId = dealer1User.Id,
                BusinessName = "Prime Motor Group",
                RegistrationNumber = "DL-CA-99201",
                ContactPersonName = "Marcus Vance",
                PhoneNumber = "+1 (310) 555-0192",
                Address = "4500 Wilshire Blvd",
                City = "Los Angeles",
                LogoUrl = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341?w=200",
                Description = "Southern California's premier dealer for verified pre-owned luxury and sports vehicles.",
                ApprovalStatus = DealerApprovalStatus.Approved,
                ApprovedAt = DateTime.UtcNow.AddMonths(-4),
                SubscriptionTier = SubscriptionTier.Standard,
                MaxListingLimit = 25,
                Rating = 4.9,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            };

            d2 = new DealerProfile
            {
                UserId = dealer2User.Id,
                BusinessName = "Apex Prestige Automotive",
                RegistrationNumber = "DL-CA-88432",
                ContactPersonName = "Elena Rostova",
                PhoneNumber = "+1 (415) 555-7821",
                Address = "1200 Van Ness Ave",
                City = "San Francisco",
                LogoUrl = "https://images.unsplash.com/photo-1563986768609-322da13575f3?w=200",
                Description = "Exclusive showroom for exotic supercars, grand tourers, and performance electrics.",
                ApprovalStatus = DealerApprovalStatus.Approved,
                ApprovedAt = DateTime.UtcNow.AddMonths(-3),
                SubscriptionTier = SubscriptionTier.Premium,
                MaxListingLimit = 100,
                Rating = 5.0,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };

            d3 = new DealerProfile
            {
                UserId = dealer3User.Id,
                BusinessName = "Metro Auto Hub",
                RegistrationNumber = "DL-CA-77114",
                ContactPersonName = "David Chen",
                PhoneNumber = "+1 (619) 555-3490",
                Address = "880 Pacific Highway",
                City = "San Diego",
                LogoUrl = "https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=200",
                Description = "Family-owned certified pre-owned vehicles, hybrid crossovers, and commuter sedans.",
                ApprovalStatus = DealerApprovalStatus.Approved,
                ApprovedAt = DateTime.UtcNow.AddMonths(-2),
                SubscriptionTier = SubscriptionTier.Free,
                MaxListingLimit = 5,
                Rating = 4.7,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };

            d4 = new DealerProfile
            {
                UserId = dealer4User.Id,
                BusinessName = "Pacific Coast Motors",
                RegistrationNumber = "DL-WA-55421",
                ContactPersonName = "Liam O'Connor",
                PhoneNumber = "+1 (206) 555-8912",
                Address = "3400 Westlake Ave N",
                City = "Seattle",
                LogoUrl = "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=200",
                Description = "Pacific Northwest specialty AWD vehicles, rugged SUVs, and electric crossovers.",
                ApprovalStatus = DealerApprovalStatus.Pending, // Pending moderation queue
                ApprovalRemarks = "Pending business license and dealer bond verification.",
                SubscriptionTier = SubscriptionTier.Standard,
                MaxListingLimit = 25,
                Rating = 4.6,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            context.DealerProfiles.AddRange(d1, d2, d3, d4);
            await context.SaveChangesAsync();
        }
        else
        {
            d1 = context.DealerProfiles.FirstOrDefault(d => d.BusinessName == "Prime Motor Group");
            d2 = context.DealerProfiles.FirstOrDefault(d => d.BusinessName == "Apex Prestige Automotive");
            d3 = context.DealerProfiles.FirstOrDefault(d => d.BusinessName == "Metro Auto Hub");
        }

        // 5. Seed Customer Users & Preferences
        var customer1 = await EnsureUserAsync(userManager, "john@example.com", "John Doe", UserRole.Customer, "Customer@123", "Los Angeles");
        var customer2 = await EnsureUserAsync(userManager, "sarah@example.com", "Sarah Jenkins", UserRole.Customer, "Customer@123", "San Francisco");

        if (!context.CustomerPreferences.Any(p => p.CustomerId == customer1.Id))
        {
            context.CustomerPreferences.Add(new CustomerPreference
            {
                CustomerId = customer1.Id,
                MinBudget = 40000m,
                MaxBudget = 85000m,
                MinYear = 2021,
                MaxMileage = 40000,
                PreferredCity = "Los Angeles",
                PreferredMakesJson = JsonSerializer.Serialize(new List<string> { "Tesla", "BMW", "Porsche", "Audi" }),
                PreferredBodyTypesJson = JsonSerializer.Serialize(new List<BodyType> { BodyType.Sedan, BodyType.Coupe }),
                PreferredFuelTypesJson = JsonSerializer.Serialize(new List<FuelType> { FuelType.Electric, FuelType.Petrol }),
                PreferredTransmissionsJson = JsonSerializer.Serialize(new List<TransmissionType> { TransmissionType.Automatic, TransmissionType.DualClutch }),
                UpdatedAt = DateTime.UtcNow
            });
        }

        if (!context.CustomerPreferences.Any(p => p.CustomerId == customer2.Id))
        {
            context.CustomerPreferences.Add(new CustomerPreference
            {
                CustomerId = customer2.Id,
                MinBudget = 30000m,
                MaxBudget = 70000m,
                MinYear = 2022,
                MaxMileage = 35000,
                PreferredCity = "San Francisco",
                PreferredMakesJson = JsonSerializer.Serialize(new List<string> { "Toyota", "Volvo", "Mercedes-Benz", "Ford" }),
                PreferredBodyTypesJson = JsonSerializer.Serialize(new List<BodyType> { BodyType.SUV, BodyType.Crossover }),
                PreferredFuelTypesJson = JsonSerializer.Serialize(new List<FuelType> { FuelType.Hybrid, FuelType.Electric, FuelType.PlugInHybrid }),
                PreferredTransmissionsJson = JsonSerializer.Serialize(new List<TransmissionType> { TransmissionType.Automatic, TransmissionType.CVT }),
                UpdatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        // 6. Seed Vehicle Inventory
        if (!context.VehicleListings.Any() && d1 != null && d2 != null && d3 != null)
        {
            var listings = new List<VehicleListing>
            {
                // Dealer 1 (Prime Motor Group)
                new()
                {
                    DealerId = d1.Id,
                    Make = "Tesla",
                    Model = "Model S Plaid",
                    Year = 2023,
                    Price = 78900m,
                    Mileage = 12400,
                    BodyType = BodyType.Sedan,
                    FuelType = FuelType.Electric,
                    Transmission = TransmissionType.Automatic,
                    Color = "Midnight Silver Metallic",
                    EngineCapacity = "Tri-Motor AWD (1,020 HP)",
                    Horsepower = 1020,
                    SeatingCapacity = 5,
                    Vin = "5YJSA1E68PF992812",
                    Description = "Immaculate Tesla Model S Plaid with Full Self-Driving Capability package, 21-inch Arachnid wheels, Cream premium interior with carbon fiber decor, and yoke steering.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "Full Self-Driving (FSD)", "Tri-Motor All-Wheel Drive", "Carbon Fiber Trim", "21\" Arachnid Wheels", "17\" Cinematic Touchscreen", "Heated & Ventilated Seats", "Subzero Weather Package", "Glass Panoramic Roof" }),
                    Status = VehicleStatus.Available,
                    ViewsCount = 428,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1617788138017-80ad40651399?w=1000", IsPrimary = true, DisplayOrder = 1 },
                        new() { ImageUrl = "https://images.unsplash.com/photo-1560958089-b8a1929cea89?w=1000", IsPrimary = false, DisplayOrder = 2 },
                        new() { ImageUrl = "https://images.unsplash.com/photo-1536700503339-1e4b06520771?w=1000", IsPrimary = false, DisplayOrder = 3 }
                    }
                },
                new()
                {
                    DealerId = d1.Id,
                    Make = "BMW",
                    Model = "M3 Competition xDrive",
                    Year = 2023,
                    Price = 82500m,
                    Mileage = 8900,
                    BodyType = BodyType.Sedan,
                    FuelType = FuelType.Petrol,
                    Transmission = TransmissionType.Automatic,
                    Color = "Isle of Man Green",
                    EngineCapacity = "3.0L BMW M TwinPower Turbo Inline-6",
                    Horsepower = 503,
                    SeatingCapacity = 5,
                    Vin = "WBA33AY08PFP44192",
                    Description = "Stunning BMW M3 Competition in flagship Isle of Man Green. Executive Package, Carbon Bucket Seats, Harman Kardon surround audio, and M Driver's package.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "M xDrive All-Wheel Drive", "M Carbon Bucket Seats", "Executive Package", "Harman Kardon Audio", "Head-Up Display", "Laserlight Headlamps", "Wireless Apple CarPlay" }),
                    Status = VehicleStatus.Available,
                    ViewsCount = 590,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1555215695-3004980ad54e?w=1000", IsPrimary = true, DisplayOrder = 1 },
                        new() { ImageUrl = "https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=1000", IsPrimary = false, DisplayOrder = 2 }
                    }
                },
                new()
                {
                    DealerId = d1.Id,
                    Make = "Audi",
                    Model = "RS6 Avant",
                    Year = 2022,
                    Price = 108000m,
                    Mileage = 16200,
                    BodyType = BodyType.Wagon,
                    FuelType = FuelType.Petrol,
                    Transmission = TransmissionType.Automatic,
                    Color = "Nardo Gray",
                    EngineCapacity = "4.0L Twin-Turbocharged V8 MHEV",
                    Horsepower = 591,
                    SeatingCapacity = 5,
                    Vin = "WAUZZZF27NN018239",
                    Description = "The ultimate high-performance wagon. Nardo Gray RS6 Avant with Black Optic package, dynamic sport exhaust, and Bang & Olufsen 3D Advanced sound system.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "Quattro All-Wheel Drive", "Dynamic Ride Control", "Black Optic Package", "Sport Exhaust", "Bang & Olufsen 3D Audio", "Ceramic Brakes" }),
                    Status = VehicleStatus.Available,
                    ViewsCount = 380,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?w=1000", IsPrimary = true, DisplayOrder = 1 }
                    }
                },
                new()
                {
                    DealerId = d1.Id,
                    Make = "Ford",
                    Model = "Mustang Mach-E GT",
                    Year = 2023,
                    Price = 52900m,
                    Mileage = 14500,
                    BodyType = BodyType.SUV,
                    FuelType = FuelType.Electric,
                    Transmission = TransmissionType.Automatic,
                    Color = "Cyber Orange Metallic",
                    EngineCapacity = "Extended Range Dual-Motor AWD",
                    Horsepower = 480,
                    SeatingCapacity = 5,
                    Vin = "3FMTK4SE0PMA88219",
                    Description = "Ford Mustang Mach-E GT with MagneRide Damping System, Ford BlueCruise hands-free driving, and panoramic glass roof.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "BlueCruise Hands-Free Driving", "MagneRide Damping", "Panoramic Fixed-Glass Roof", "B&O Sound System by Bang & Olufsen", "360-Degree Camera" }),
                    Status = VehicleStatus.Available,
                    ViewsCount = 275,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1541899481282-d53bffe3c35d?w=1000", IsPrimary = true, DisplayOrder = 1 }
                    }
                },

                // Dealer 2 (Apex Prestige Automotive)
                new()
                {
                    DealerId = d2.Id,
                    Make = "Porsche",
                    Model = "911 GT3 (992)",
                    Year = 2023,
                    Price = 224900m,
                    Mileage = 3400,
                    BodyType = BodyType.Coupe,
                    FuelType = FuelType.Petrol,
                    Transmission = TransmissionType.DualClutch,
                    Color = "Shark Blue",
                    EngineCapacity = "4.0L Naturally Aspirated Boxer-6",
                    Horsepower = 502,
                    SeatingCapacity = 2,
                    Vin = "WP0AC2A98PS299318",
                    Description = "Naturally aspirated perfection. 992 GT3 in iconic Shark Blue with Front Axle Lift system, Chrono package, Full Bucket Seats, and carbon fiber roof.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "PDK Dual-Clutch 7-Speed", "Front Axle Lift System", "Chrono Package", "Full Carbon Bucket Seats", "Carbon Fiber Roof", "Motorsport Swan-Neck Wing" }),
                    Status = VehicleStatus.Available,
                    ViewsCount = 1120,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=1000", IsPrimary = true, DisplayOrder = 1 },
                        new() { ImageUrl = "https://images.unsplash.com/photo-1614162692292-7ac56d7f7f1e?w=1000", IsPrimary = false, DisplayOrder = 2 }
                    }
                },
                new()
                {
                    DealerId = d2.Id,
                    Make = "Porsche",
                    Model = "Taycan Turbo S",
                    Year = 2022,
                    Price = 119500m,
                    Mileage = 18200,
                    BodyType = BodyType.Sedan,
                    FuelType = FuelType.Electric,
                    Transmission = TransmissionType.Automatic,
                    Color = "Frozen Blue Metallic",
                    EngineCapacity = "Permanent Magnet Synchronous Motors",
                    Horsepower = 750,
                    SeatingCapacity = 4,
                    Vin = "WP0AA2Y14NSA77291",
                    Description = "Uncompromising electric acceleration. Porsche Taycan Turbo S with Porsche Ceramic Composite Brakes (PCCB), Rear-Axle Steering, and Burmester High-End 3D Surround.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "Overboost Power with Launch Control", "PCCB Ceramic Brakes", "Burmester 3D Surround", "Passenger Display", "Adaptive Air Suspension" }),
                    Status = VehicleStatus.Available,
                    ViewsCount = 670,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1580273916550-e323be2ae537?w=1000", IsPrimary = true, DisplayOrder = 1 }
                    }
                },
                new()
                {
                    DealerId = d2.Id,
                    Make = "Mercedes-Benz",
                    Model = "G63 AMG 4MATIC",
                    Year = 2022,
                    Price = 179000m,
                    Mileage = 21000,
                    BodyType = BodyType.SUV,
                    FuelType = FuelType.Petrol,
                    Transmission = TransmissionType.Automatic,
                    Color = "Obsidian Black Metallic",
                    EngineCapacity = "Handcrafted AMG 4.0L V8 Biturbo",
                    Horsepower = 577,
                    SeatingCapacity = 5,
                    Vin = "W1N4632761X399812",
                    Description = "The iconic G-Wagon. AMG G63 with Night Package Plus, G manufaktur diamond-quilted Nappa leather, AMG performance exhaust, and 22\" forged cross-spoke wheels.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "AMG 4MATIC All-Wheel Drive", "3 Lockable Differentials", "AMG Night Package Plus", "Burmester Surround Sound", "G Manufaktur Exclusive Interior" }),
                    Status = VehicleStatus.Available,
                    ViewsCount = 890,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-18),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1520031441872-265e4ff70366?w=1000", IsPrimary = true, DisplayOrder = 1 }
                    }
                },
                new()
                {
                    DealerId = d2.Id,
                    Make = "Chevrolet",
                    Model = "Corvette Z06 3LZ",
                    Year = 2023,
                    Price = 138500m,
                    Mileage = 4100,
                    BodyType = BodyType.Coupe,
                    FuelType = FuelType.Petrol,
                    Transmission = TransmissionType.DualClutch,
                    Color = "Torch Red",
                    EngineCapacity = "5.5L Flat-Plane Crank LT6 V8",
                    Horsepower = 670,
                    SeatingCapacity = 2,
                    Vin = "1G1YC2D37P5600192",
                    Description = "Mid-engine supercar performance with the highest-horsepower naturally aspirated production V8 ever built. Z07 Performance Package with carbon ceramic brakes and aero.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "Z07 Performance Package", "Brembo Carbon Ceramic Brakes", "Carbon Fiber Aero Package", "Performance Data Recorder", "GT2 Competition Seats" }),
                    Status = VehicleStatus.Reserved,
                    ViewsCount = 490,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=1000", IsPrimary = true, DisplayOrder = 1 }
                    }
                },

                // Dealer 3 (Metro Auto Hub)
                new()
                {
                    DealerId = d3.Id,
                    Make = "Toyota",
                    Model = "RAV4 Prime XSE",
                    Year = 2023,
                    Price = 46900m,
                    Mileage = 18400,
                    BodyType = BodyType.SUV,
                    FuelType = FuelType.PlugInHybrid,
                    Transmission = TransmissionType.CVT,
                    Color = "Blizzard Pearl / Black Metallic",
                    EngineCapacity = "2.5L 4-Cylinder Plug-In Hybrid",
                    Horsepower = 302,
                    SeatingCapacity = 5,
                    Vin = "JTMEB3FV7PD019283",
                    Description = "Best-in-class plug-in hybrid SUV. 42 miles of pure EV range, 302 horsepower AWD acceleration, Premium Package, and JBL 11-speaker audio.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "42-Mile All-Electric Range", "Electronic On-Demand AWD", "JBL Premium Audio", "Head-Up Display", "Panoramic Moonroof", "Toyota Safety Sense 2.5+" }),
                    Status = VehicleStatus.Available,
                    ViewsCount = 310,
                    IsFeatured = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1581540222194-0def2dda95b8?w=1000", IsPrimary = true, DisplayOrder = 1 }
                    }
                },
                new()
                {
                    DealerId = d3.Id,
                    Make = "Hyundai",
                    Model = "Ioniq 5 Limited AWD",
                    Year = 2023,
                    Price = 44500m,
                    Mileage = 15800,
                    BodyType = BodyType.Crossover,
                    FuelType = FuelType.Electric,
                    Transmission = TransmissionType.Automatic,
                    Color = "Cyber Gray",
                    EngineCapacity = "77.4 kWh Dual Motor AWD",
                    Horsepower = 320,
                    SeatingCapacity = 5,
                    Vin = "KM8KRDAF4PU118274",
                    Description = "Award-winning EV with ultra-fast 800V charging (10% to 80% in 18 minutes), Relaxation Comfort driver seat, augmented reality Head-Up Display, and Vision roof.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "800V Ultra-Fast Charging", "Vehicle-to-Load (V2L) Power", "Relaxation Comfort Driver Seat", "Augmented Reality HUD", "Highway Driving Assist II" }),
                    Status = VehicleStatus.Available,
                    ViewsCount = 260,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-14),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1549399542-7e3f8b79c341?w=1000", IsPrimary = true, DisplayOrder = 1 }
                    }
                },
                new()
                {
                    DealerId = d3.Id,
                    Make = "Honda",
                    Model = "Civic Type R (FL5)",
                    Year = 2023,
                    Price = 47800m,
                    Mileage = 6200,
                    BodyType = BodyType.Hatchback,
                    FuelType = FuelType.Petrol,
                    Transmission = TransmissionType.Manual,
                    Color = "Championship White",
                    EngineCapacity = "2.0L Turbocharged VTEC 4-Cylinder",
                    Horsepower = 315,
                    SeatingCapacity = 4,
                    Vin = "JHMFL5G44PX001928",
                    Description = "The ultimate hot hatch with 6-speed manual with rev-matching, Brembo 4-piston aluminum front calipers, Championship White with vivid red Alcantara sport seats.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "6-Speed Manual with Rev-Match", "Helical Limited-Slip Differential", "Adaptive Damper System", "Honda LogR Datalogger", "Brembo Brakes" }),
                    Status = VehicleStatus.Available,
                    ViewsCount = 520,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-9),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1605559424843-9e4c228bf1c2?w=1000", IsPrimary = true, DisplayOrder = 1 }
                    }
                },
                new()
                {
                    DealerId = d3.Id,
                    Make = "Volvo",
                    Model = "XC90 Recharge Ultimate",
                    Year = 2022,
                    Price = 64900m,
                    Mileage = 28400,
                    BodyType = BodyType.SUV,
                    FuelType = FuelType.PlugInHybrid,
                    Transmission = TransmissionType.Automatic,
                    Color = "Crystal White Metallic",
                    EngineCapacity = "2.0L Turbocharged + Electric Motor T8 AWD",
                    Horsepower = 455,
                    SeatingCapacity = 7,
                    Vin = "YV4BR00K9N1892102",
                    Description = "Scandinavian luxury and safety. 7-passenger plug-in hybrid SUV with Bowers & Wilkins premium sound, air suspension, graphical Head-Up Display, and Orrefors crystal gear shifter.",
                    FeaturesJson = JsonSerializer.Serialize(new List<string> { "7-Passenger Seating", "Bowers & Wilkins Sound System", "Orrefors Crystal Shifter", "Air Suspension", "Pilot Assist", "360-Degree Camera" }),
                    Status = VehicleStatus.Sold,
                    ViewsCount = 340,
                    IsFeatured = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-40),
                    Images = new List<VehicleImage>
                    {
                        new() { ImageUrl = "https://images.unsplash.com/photo-1542282088-72c9c27ed0cd?w=1000", IsPrimary = true, DisplayOrder = 1 }
                    }
                }
            };

            context.VehicleListings.AddRange(listings);
            await context.SaveChangesAsync();

            // 7. Seed Bookings & Inquiries
            var tesla = listings.First(l => l.Make == "Tesla");
            var bmw = listings.First(l => l.Make == "BMW");
            var toyota = listings.First(l => l.Make == "Toyota");

            context.TestDriveBookings.AddRange(
                new TestDriveBooking
                {
                    VehicleId = tesla.Id,
                    DealerId = d1.Id,
                    CustomerId = customer1.Id,
                    ScheduledDate = DateTime.UtcNow.AddDays(2),
                    PreferredTimeSlot = "11:00 AM - 12:00 PM",
                    Notes = "Interested in testing the yoke steering and acceleration on highway.",
                    CustomerContactNumber = "+1 (310) 555-4421",
                    Status = BookingStatus.Approved,
                    DealerNotes = "Approved! Vehicle will be fully charged and staged at our main showroom entrance.",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new TestDriveBooking
                {
                    VehicleId = bmw.Id,
                    DealerId = d1.Id,
                    CustomerId = customer1.Id,
                    ScheduledDate = DateTime.UtcNow.AddDays(4),
                    PreferredTimeSlot = "02:00 PM - 03:00 PM",
                    Notes = "Looking to compare M3 handling against my previous M4.",
                    CustomerContactNumber = "+1 (310) 555-4421",
                    Status = BookingStatus.Requested,
                    CreatedAt = DateTime.UtcNow
                },
                new TestDriveBooking
                {
                    VehicleId = toyota.Id,
                    DealerId = d3.Id,
                    CustomerId = customer2.Id,
                    ScheduledDate = DateTime.UtcNow.AddDays(3),
                    PreferredTimeSlot = "10:00 AM - 11:00 AM",
                    Notes = "Would love to test child seat fitment in second row.",
                    CustomerContactNumber = "+1 (415) 555-6677",
                    Status = BookingStatus.Requested,
                    CreatedAt = DateTime.UtcNow.AddHours(-12)
                }
            );

            context.LeadInquiries.AddRange(
                new LeadInquiry
                {
                    VehicleId = tesla.Id,
                    DealerId = d1.Id,
                    CustomerId = customer1.Id,
                    CustomerName = "John Doe",
                    CustomerEmail = "john@example.com",
                    CustomerPhone = "+1 (310) 555-4421",
                    Message = "Does this Model S Plaid still have the active transferrable warranty from Tesla for battery and drive unit?",
                    Status = InquiryStatus.Contacted,
                    DealerResponse = "Yes! The battery and drive unit warranty is active through 2031 or 150k miles. Clean Carfax report ready.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new LeadInquiry
                {
                    VehicleId = toyota.Id,
                    DealerId = d3.Id,
                    CustomerId = customer2.Id,
                    CustomerName = "Sarah Jenkins",
                    CustomerEmail = "sarah@example.com",
                    CustomerPhone = "+1 (415) 555-6677",
                    Message = "Are there any dealer markups or additional documentation fees on the listed RAV4 Prime price?",
                    Status = InquiryStatus.New,
                    CreatedAt = DateTime.UtcNow.AddHours(-6)
                }
            );

            // Seed Favorites
            context.FavoriteListings.AddRange(
                new FavoriteListing { CustomerId = customer1.Id, VehicleId = tesla.Id, SavedAt = DateTime.UtcNow.AddDays(-2) },
                new FavoriteListing { CustomerId = customer1.Id, VehicleId = bmw.Id, SavedAt = DateTime.UtcNow.AddDays(-1) },
                new FavoriteListing { CustomerId = customer2.Id, VehicleId = toyota.Id, SavedAt = DateTime.UtcNow.AddDays(-3) }
            );

            await context.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        UserRole role,
        string password,
        string city)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                Role = role,
                City = city,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role.ToString());
        }
        return user;
    }
}
