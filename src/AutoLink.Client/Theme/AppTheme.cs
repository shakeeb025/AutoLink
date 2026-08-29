using MudBlazor;

namespace AutoLink.Client.Theme;

public static class AppTheme
{
    public static MudTheme CustomTheme => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#E50914",          // Performance Crimson Red
            Secondary = "#0F172A",        // Deep Onyx Charcoal
            Tertiary = "#DC2626",         // Vivid Racing Red
            AppbarBackground = "rgba(255, 255, 255, 0.90)", // Frosted Pure White
            AppbarText = "#09090B",       // Pitch Black
            Background = "#F8FAFC",       // Ultra Clean Canvas
            Surface = "#FFFFFF",          // Crisp White Card
            DrawerBackground = "#FFFFFF",
            DrawerText = "#09090B",
            DrawerIcon = "#71717A",
            TextPrimary = "#09090B",
            TextSecondary = "#71717A",
            LinesDefault = "rgba(0, 0, 0, 0.08)",
            LinesInputs = "rgba(0, 0, 0, 0.16)",
            Divider = "rgba(0, 0, 0, 0.06)",
            Success = "#16A34A",          // Forest Green
            Warning = "#F59E0B",          // Amber
            Error = "#E50914",            // Crimson Red
            Info = "#0F172A"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#FF2E4D",          // Electric Neon Crimson
            Secondary = "#E2E8F0",        // Platinum Light
            Tertiary = "#F43F5E",         // Rose Red
            AppbarBackground = "rgba(10, 10, 12, 0.90)", // Deep Obsidian Frosted
            AppbarText = "#F8FAFC",
            Background = "#08080A",       // Pitch OLED Black
            Surface = "#121215",          // Deep Obsidian Card
            DrawerBackground = "#121215",
            DrawerText = "#F8FAFC",
            DrawerIcon = "#A1A1AA",
            TextPrimary = "#F8FAFC",
            TextSecondary = "#A1A1AA",
            LinesDefault = "rgba(255, 255, 255, 0.08)",
            LinesInputs = "rgba(255, 255, 255, 0.18)",
            Divider = "rgba(255, 255, 255, 0.08)",
            Success = "#22C55E",
            Warning = "#FBBF24",
            Error = "#FF2E4D",
            Info = "#FF2E4D"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px",
            AppbarHeight = "68px"
        },
        Typography = new Typography
        {
            Default = new()
            {
                FontFamily = new[] { "-apple-system", "BlinkMacSystemFont", "SF Pro Display", "SF Pro Text", "Inter", "Helvetica Neue", "sans-serif" }
            },
            H1 = new() { FontWeight = 800, LetterSpacing = "-0.03em" },
            H2 = new() { FontWeight = 800, LetterSpacing = "-0.025em" },
            H3 = new() { FontWeight = 700, LetterSpacing = "-0.02em" },
            H4 = new() { FontWeight = 700, LetterSpacing = "-0.015em" },
            H5 = new() { FontWeight = 600, LetterSpacing = "-0.01em" },
            H6 = new() { FontWeight = 600 },
            Subtitle1 = new() { FontWeight = 500 },
            Subtitle2 = new() { FontWeight = 500 },
            Button = new() { FontWeight = 600, TextTransform = "none", LetterSpacing = "-0.01em" }
        }
    };
}
