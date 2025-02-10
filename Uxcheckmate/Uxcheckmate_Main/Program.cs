using Uxcheckmate_Main.DAL.Abstract;
using Uxcheckmate_Main.DAL.Concrete;
using Uxcheckmate_Main.Models;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;
using System.Net.Http;
using Uxcheckmate_Main.Services; // Ensure this namespace is correct for WebScraperService

namespace Uxcheckmate_Main;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<UxCheckmateDbContext>(options =>
            options.UseLazyLoadingProxies()
                   .UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));

        // Register HttpClient and WebScraperService
        builder.Services.AddHttpClient<WebScraperService>();

        var app = builder.Build();

        // Middleware: Custom error handling
        app.UseStatusCodePagesWithReExecute("/Home/ErrorPage");

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();  // Ensure static files (CSS, JS, images) are served

        app.UseRouting();
        app.UseAuthorization();

        // Map default route
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
