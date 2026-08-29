using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using AutoLink.Core.Entities;
using AutoLink.Core.Interfaces;
using AutoLink.Infrastructure.Data;
using AutoLink.Shared.DTOs;
using AutoLink.Shared.Enums;
using AutoLink.Shared.Models;

namespace AutoLink.Infrastructure.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly AutoLinkDbContext _context;

    public VehicleRepository(AutoLinkDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<VehicleDto>> GetVehiclesAsync(VehicleFilterDto filter, string? currentUserId = null)
    {
        var query = _context.VehicleListings
            .Include(v => v.Dealer)
            .Include(v => v.Images)
            .AsQueryable();

        // Status filter
        if (filter.Status.HasValue)
        {
            query = query.Where(v => v.Status == filter.Status.Value);
        }
        else
        {
            query = query.Where(v => v.Status == VehicleStatus.Available);
        }

        // Dealer filter
        if (filter.DealerId.HasValue)
        {
            query = query.Where(v => v.DealerId == filter.DealerId.Value);
        }

        // Search term (Make, Model, Color, Description)
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            string term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(v => v.Make.ToLower().Contains(term) ||
                                     v.Model.ToLower().Contains(term) ||
                                     v.Color.ToLower().Contains(term) ||
                                     v.Description.ToLower().Contains(term));
        }

        // Make & Model
        if (!string.IsNullOrWhiteSpace(filter.Make))
        {
            query = query.Where(v => v.Make.ToLower() == filter.Make.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(filter.Model))
        {
            query = query.Where(v => v.Model.ToLower().Contains(filter.Model.Trim().ToLower()));
        }

        // Price range
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(v => v.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(v => v.Price <= filter.MaxPrice.Value);
        }

        // Year range
        if (filter.MinYear.HasValue)
        {
            query = query.Where(v => v.Year >= filter.MinYear.Value);
        }

        if (filter.MaxYear.HasValue)
        {
            query = query.Where(v => v.Year <= filter.MaxYear.Value);
        }

        // Max Mileage
        if (filter.MaxMileage.HasValue)
        {
            query = query.Where(v => v.Mileage <= filter.MaxMileage.Value);
        }

        // Body Type
        if (filter.BodyType.HasValue)
        {
            query = query.Where(v => v.BodyType == filter.BodyType.Value);
        }

        // Fuel Type
        if (filter.FuelType.HasValue)
        {
            query = query.Where(v => v.FuelType == filter.FuelType.Value);
        }

        // Transmission
        if (filter.Transmission.HasValue)
        {
            query = query.Where(v => v.Transmission == filter.Transmission.Value);
        }

        // City
        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query = query.Where(v => v.Dealer.City.ToLower().Contains(filter.City.Trim().ToLower()));
        }

        // Sorting
        query = filter.SortBy switch
        {
            "PriceAsc" => query.OrderBy(v => v.Price),
            "PriceDesc" => query.OrderByDescending(v => v.Price),
            "YearDesc" => query.OrderByDescending(v => v.Year),
            "MileageAsc" => query.OrderBy(v => v.Mileage),
            "Popular" => query.OrderByDescending(v => v.ViewsCount),
            _ => query.OrderByDescending(v => v.IsFeatured).ThenByDescending(v => v.CreatedAt)
        };

        int totalCount = await query.CountAsync();
        int pageNumber = Math.Max(1, filter.PageNumber);
        int pageSize = Math.Max(1, Math.Min(100, filter.PageSize));

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Customer favorites lookup
        HashSet<int> userFavs = new();
        if (!string.IsNullOrEmpty(currentUserId))
        {
            var favIds = await _context.FavoriteListings
                .Where(f => f.CustomerId == currentUserId)
                .Select(f => f.VehicleId)
                .ToListAsync();
            userFavs = new HashSet<int>(favIds);
        }

        var dtos = items.Select(v => new VehicleDto
        {
            Id = v.Id,
            DealerId = v.DealerId,
            DealerName = v.Dealer?.BusinessName ?? "AutoLink Dealer",
            DealerCity = v.Dealer?.City ?? "Certified Location",
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
            IsFavorite = userFavs.Contains(v.Id),
            PrimaryImageUrl = v.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                             ?? v.Images.FirstOrDefault()?.ImageUrl
                             ?? "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800",
            Images = v.Images.Select(i => new VehicleImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            }).ToList()
        }).ToList();

        return new PagedResult<VehicleDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<VehicleListing?> GetByIdAsync(int id)
    {
        return await _context.VehicleListings
            .Include(v => v.Dealer)
            .Include(v => v.Images)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<VehicleDetailDto?> GetDetailByIdAsync(int id, string? currentUserId = null)
    {
        var v = await _context.VehicleListings
            .Include(v => v.Dealer)
                .ThenInclude(d => d.User)
            .Include(v => v.Images)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (v == null) return null;

        bool isFav = false;
        if (!string.IsNullOrEmpty(currentUserId))
        {
            isFav = await _context.FavoriteListings.AnyAsync(f => f.CustomerId == currentUserId && f.VehicleId == id);
        }

        List<string> features = new();
        if (!string.IsNullOrEmpty(v.FeaturesJson))
        {
            try { features = JsonSerializer.Deserialize<List<string>>(v.FeaturesJson) ?? new(); } catch { }
        }

        var primaryImg = v.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                         ?? v.Images.FirstOrDefault()?.ImageUrl
                         ?? "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800";

        return new VehicleDetailDto
        {
            Id = v.Id,
            DealerId = v.DealerId,
            DealerName = v.Dealer?.BusinessName ?? "AutoLink Certified",
            DealerCity = v.Dealer?.City ?? "Certified City",
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
            PrimaryImageUrl = primaryImg,
            ViewsCount = v.ViewsCount,
            CreatedAt = v.CreatedAt,
            IsFeatured = v.IsFeatured,
            IsFavorite = isFav,
            Description = v.Description,
            EngineCapacity = v.EngineCapacity,
            Horsepower = v.Horsepower,
            SeatingCapacity = v.SeatingCapacity,
            Vin = v.Vin,
            Features = features,
            Images = v.Images.Select(i => new VehicleImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            Dealer = v.Dealer != null ? new DealerProfileDto
            {
                Id = v.Dealer.Id,
                UserId = v.Dealer.UserId,
                BusinessName = v.Dealer.BusinessName,
                RegistrationNumber = v.Dealer.RegistrationNumber,
                ContactPersonName = v.Dealer.ContactPersonName,
                Email = v.Dealer.User?.Email ?? string.Empty,
                PhoneNumber = v.Dealer.PhoneNumber,
                Address = v.Dealer.Address,
                City = v.Dealer.City,
                LogoUrl = v.Dealer.LogoUrl,
                Description = v.Dealer.Description,
                ApprovalStatus = v.Dealer.ApprovalStatus,
                SubscriptionTier = v.Dealer.SubscriptionTier,
                SubscriptionExpiry = v.Dealer.SubscriptionExpiry,
                ActiveListingsCount = v.Dealer.Listings?.Count ?? 0,
                MaxListingLimit = v.Dealer.MaxListingLimit,
                Rating = v.Dealer.Rating,
                CreatedAt = v.Dealer.CreatedAt
            } : new DealerProfileDto()
        };
    }

    public async Task<List<VehicleListing>> GetAllAvailableListingsAsync()
    {
        return await _context.VehicleListings
            .Where(v => v.Status == VehicleStatus.Available)
            .Include(v => v.Dealer)
            .Include(v => v.Images)
            .ToListAsync();
    }

    public async Task<List<VehicleDto>> GetSellerListingsAsync(int dealerId, VehicleStatus? status = null)
    {
        var query = _context.VehicleListings
            .Include(v => v.Dealer)
            .Include(v => v.Images)
            .Where(v => v.DealerId == dealerId);

        if (status.HasValue)
        {
            query = query.Where(v => v.Status == status.Value);
        }

        var items = await query.OrderByDescending(v => v.CreatedAt).ToListAsync();

        return items.Select(v => new VehicleDto
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
                             ?? "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=800",
            Images = v.Images.Select(i => new VehicleImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            }).ToList()
        }).ToList();
    }

    public async Task<VehicleListing> AddAsync(VehicleListing vehicle)
    {
        _context.VehicleListings.Add(vehicle);
        await _context.SaveChangesAsync();
        return vehicle;
    }

    public async Task UpdateAsync(VehicleListing vehicle)
    {
        _context.VehicleListings.Update(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.VehicleListings.FindAsync(id);
        if (item != null)
        {
            _context.VehicleListings.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementViewsAsync(int id)
    {
        var vehicle = await _context.VehicleListings.FindAsync(id);
        if (vehicle != null)
        {
            vehicle.ViewsCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.VehicleListings.AnyAsync(v => v.Id == id);
    }

    public async Task<List<FavoriteListing>> GetCustomerFavoritesAsync(string customerId)
    {
        return await _context.FavoriteListings
            .Where(f => f.CustomerId == customerId)
            .Include(f => f.Vehicle)
                .ThenInclude(v => v.Dealer)
            .Include(f => f.Vehicle)
                .ThenInclude(v => v.Images)
            .ToListAsync();
    }

    public async Task<bool> ToggleFavoriteAsync(string customerId, int vehicleId)
    {
        var fav = await _context.FavoriteListings
            .FirstOrDefaultAsync(f => f.CustomerId == customerId && f.VehicleId == vehicleId);

        if (fav != null)
        {
            _context.FavoriteListings.Remove(fav);
            await _context.SaveChangesAsync();
            return false; // Removed
        }
        else
        {
            _context.FavoriteListings.Add(new FavoriteListing
            {
                CustomerId = customerId,
                VehicleId = vehicleId,
                SavedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true; // Added
        }
    }

    public async Task<bool> IsFavoriteAsync(string customerId, int vehicleId)
    {
        return await _context.FavoriteListings.AnyAsync(f => f.CustomerId == customerId && f.VehicleId == vehicleId);
    }
}
