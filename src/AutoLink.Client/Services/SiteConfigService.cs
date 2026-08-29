using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoLink.Client.Services;

public class SiteConfig
{
    public string SiteTitle { get; set; } = "AutoLink";
    public string SiteTagline { get; set; } = "PREMIER MOTOR NETWORK";
    public string HeroHeadline { get; set; } = "The Apex of Automotive Luxury & Performance";
    public string HeroSubheadline { get; set; } = "Discover, compare, and schedule test drives with verified premier dealerships nationwide.";
    public string HeroBannerImageUrl { get; set; } = "https://images.unsplash.com/photo-1617814076367-b759c7d7e738?q=80&w=2000&auto=format&fit=crop";
    public string PrimaryColor { get; set; } = "#E50914"; // Performance Crimson Red
    public string ContactEmail { get; set; } = "concierge@autolink.com";
    public string ContactPhone { get; set; } = "+1 (800) 555-AUTO";
    public string CopyrightText { get; set; } = "© 2026 AutoLink Enterprise Automotive Group. All rights reserved.";
}

public class BannerPreset
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class SiteConfigService
{
    private readonly LocalStorageService _localStorage;
    private const string StorageKey = "autoLink_siteConfig";

    public SiteConfig Config { get; private set; } = new();
    public event Action? OnConfigChanged;

    public List<BannerPreset> Presets { get; } = new()
    {
        new BannerPreset
        {
            Title = "Porsche 911 GT3 RS",
            Subtitle = "Motorsport Aerodynamics & Pure Passion",
            Category = "Supercar",
            ImageUrl = "https://images.unsplash.com/photo-1617814076367-b759c7d7e738?q=80&w=2000&auto=format&fit=crop"
        },
        new BannerPreset
        {
            Title = "Tesla Model S Plaid & Cyber Dark",
            Subtitle = "Next-Generation Electric Super Performance",
            Category = "Electric",
            ImageUrl = "https://images.unsplash.com/photo-1560958089-b8a1929cea89?q=80&w=2000&auto=format&fit=crop"
        },
        new BannerPreset
        {
            Title = "BMW M8 Competition Gran Coupe",
            Subtitle = "Executive Power & Bespoke Craftsmanship",
            Category = "Luxury Sedan",
            ImageUrl = "https://images.unsplash.com/photo-1555215695-3004980ad54e?q=80&w=2000&auto=format&fit=crop"
        },
        new BannerPreset
        {
            Title = "Mercedes-AMG GT Black Series",
            Subtitle = "Handcrafted AMG V8 Twin-Turbo Beast",
            Category = "Track Focus",
            ImageUrl = "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?q=80&w=2000&auto=format&fit=crop"
        },
        new BannerPreset
        {
            Title = "Audi R8 V10 Performance",
            Subtitle = "Naturally Aspirated Legend with Quattro Drive",
            Category = "Exotic",
            ImageUrl = "https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?q=80&w=2000&auto=format&fit=crop"
        }
    };

    public SiteConfigService(LocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task InitializeAsync()
    {
        var saved = await _localStorage.GetItemAsync<SiteConfig>(StorageKey);
        if (saved != null)
        {
            Config = saved;
            OnConfigChanged?.Invoke();
        }
    }

    public async Task UpdateConfigAsync(SiteConfig newConfig)
    {
        Config = newConfig;
        await _localStorage.SetItemAsync(StorageKey, Config);
        OnConfigChanged?.Invoke();
    }

    public async Task ApplyBannerPresetAsync(BannerPreset preset)
    {
        Config.HeroBannerImageUrl = preset.ImageUrl;
        Config.HeroHeadline = preset.Title;
        Config.HeroSubheadline = preset.Subtitle;
        await _localStorage.SetItemAsync(StorageKey, Config);
        OnConfigChanged?.Invoke();
    }

    public async Task ResetToDefaultsAsync()
    {
        Config = new SiteConfig();
        await _localStorage.RemoveItemAsync(StorageKey);
        OnConfigChanged?.Invoke();
    }
}
