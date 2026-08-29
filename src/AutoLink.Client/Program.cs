using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using AutoLink.Client;
using AutoLink.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. MudBlazor UI Services
builder.Services.AddMudServices();

// 2. Local Storage & Security Interceptor
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<JwtInterceptor>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// 3. HTTP Client with Automatic JWT Interception
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddTransient(sp =>
{
    var jwtInterceptor = sp.GetRequiredService<JwtInterceptor>();
    jwtInterceptor.InnerHandler ??= new HttpClientHandler();
    return new HttpClient(jwtInterceptor)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
});

// 4. API Client Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<DealerService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<ComparisonService>();
builder.Services.AddScoped<SiteConfigService>();

await builder.Build().RunAsync();
