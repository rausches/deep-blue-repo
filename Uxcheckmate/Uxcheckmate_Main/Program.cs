using Uxcheckmate_Main.DAL.Abstract;
using Uxcheckmate_Main.DAL.Concrete;
using Uxcheckmate_Main.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using Uxcheckmate_Main.Services;
using HtmlAgilityPack;
using System.Net.Http;
using Microsoft.Build.Framework;
using QuestPDF;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Uxcheckmate_Main.Areas.Identity.Data;

namespace Uxcheckmate_Main;

public class Program
{
    public static void Main(string[] args)
    {
        QuestPDF.Settings.License = LicenseType.Community; // Add this line

        var builder = WebApplication.CreateBuilder(args);

        string openAiApiKey = builder.Configuration["OpenAiApiKey"];
        string openAiUrl = "https://api.openai.com/v1/chat/completions";

        builder.Services.AddDbContext<UxCheckmateDbContext>(options =>
                options.UseLazyLoadingProxies()
                    .UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));


        // Auth DB
        builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("AuthDBConnection"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
                ));
        builder.Services.AddDefaultIdentity<IdentityUser>(options => 
                options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<AuthDbContext>();



            builder.Services.AddHttpClient<IOpenAiService, OpenAiService>((httpClient, services) =>
            {
                string openAiUrl = "https://api.openai.com/v1/chat/completions";
                string openAiApiKey = builder.Configuration["OpenAiApiKey"];
                httpClient.BaseAddress = new Uri(openAiUrl);          
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
            
                return new OpenAiService(
                    httpClient,
                    services.GetRequiredService<ILogger<OpenAiService>>()
                );
            });

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // Register HttpClient and WebScraperService
        builder.Services.AddHttpClient<WebScraperService>();

        // Register ScreenshotService
        builder.Services.AddScoped<ScreenshotService>();

        // Register PlaywrightService
        builder.Services.AddScoped<IPlaywrightService, PlaywrightService>();
        builder.Services.AddScoped<PlaywrightService>();

        // Register Pa11yUrlBasedService and Pa11yService
        builder.Services.AddScoped<IAxeCoreService, AxeCoreService>();

        // Register ScreenshotService
        builder.Services.AddScoped<IScreenshotService, ScreenshotService>();

        builder.Services.AddScoped<PdfExportService>();

        // Register Report Services
        builder.Services.AddScoped<IReportService, ReportService>();
        builder.Services.AddScoped<IBrokenLinksService, BrokenLinksService>();
        builder.Services.AddScoped<IHeadingHierarchyService, HeadingHierarchyService>();
        builder.Services.AddScoped<IColorSchemeService, ColorSchemeService>();
        builder.Services.AddScoped<IDynamicSizingService, DynamicSizingService>();
        builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
        builder.Services.AddHttpClient<IFaviconDetectionService, FaviconDetectionService>();


        var app = builder.Build();

        // Middleware: Custom error handling
        app.UseStatusCodePagesWithRedirects("/Home/ErrorPage");

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();  // Ensure static files (CSS, JS, images) are served

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        // Map default route
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages(); // For indentity

        app.Run();
    }
}