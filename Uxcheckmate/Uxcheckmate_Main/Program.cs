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

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();

        string openAiApiKey = builder.Configuration["OpenAiApiKey"];
        string openAiUrl = "https://api.openai.com/v1/chat/completions";

        // Add swagger services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<UxCheckmateDbContext>(options =>
                options.UseLazyLoadingProxies()
                    .UseSqlServer(builder.Configuration.GetConnectionString("DBConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()));

        // Auth DB
        builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("AuthDBConnection"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
                ));
        builder.Services.AddDefaultIdentity<IdentityUser>(options => 
                options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AuthDbContext>();

        builder.Services.AddAuthentication()
            .AddGitHub(options =>
            {
                options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
                options.CallbackPath = "/signin-github";
                options.Scope.Add("user:email");
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            });

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

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
        builder.Services.AddScoped<IScreenshotService, ScreenshotService>();
        builder.Services.AddScoped<ScreenshotService>();

        // Register PlaywrightService
        builder.Services.AddScoped<IPlaywrightService, PlaywrightService>();
        builder.Services.AddScoped<PlaywrightService>();

        // Register AxeCoreService
        builder.Services.AddScoped<IAxeCoreService, AxeCoreService>();

        // Register ScreenshotService
        builder.Services.AddScoped<IScreenshotService, ScreenshotService>();
        builder.Services.AddScoped<PdfExportService>();

        // Register Captcha Service 
        builder.Services.AddScoped<ICaptchaService, CaptchaService>();

        // Register Report Services
        builder.Services.AddScoped<IReportService, ReportService>();
        builder.Services.AddScoped<IWebScraperService, WebScraperService>();
        builder.Services.AddScoped<IBrokenLinksService, BrokenLinksService>();
        builder.Services.AddScoped<IHeadingHierarchyService, HeadingHierarchyService>();
        builder.Services.AddScoped<IColorSchemeService, ColorSchemeService>();
        builder.Services.AddScoped<IMobileResponsivenessService, MobileResponsivenessService>();
        builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
        builder.Services.AddScoped<IPlaywrightScraperService, PlaywrightScraperService>();
        builder.Services.AddScoped<IPopUpsService, PopUpsService>();
        builder.Services.AddScoped<IAnimationService, AnimationService>();
        builder.Services.AddScoped<IAudioService, AudioService>();
        builder.Services.AddScoped<IScrollService, ScrollService>();
        builder.Services.AddScoped<ILayoutParsingService, LayoutParsingService>();
        builder.Services.AddScoped<IFPatternService, FPatternService>();
        builder.Services.AddScoped<IZPatternService, ZPatternService>();
        builder.Services.AddScoped<ISymmetryService, SymmetryService>();

        //Register Background Services
        builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        builder.Services.AddHostedService<QueueService>();
        builder.Services.AddHostedService<ReportCleanupService>();

        // Register and Config Jira
        builder.Services.Configure<JiraSettings>(builder.Configuration.GetSection("Jira"));
        builder.Services.AddHttpClient<IJiraService, JiraService>();
        builder.Services.AddHttpClient<IPlaywrightApiService, PlaywrightApiService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthorization();
        app.MapControllers();

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
        app.UseSession();
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