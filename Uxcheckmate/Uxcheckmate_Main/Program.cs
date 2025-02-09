using Uxcheckmate_Main.DAL.Abstract;
using Uxcheckmate_Main.DAL.Concrete;
using Uxcheckmate_Main.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using Uxcheckmate_Main.Services;

namespace Uxcheckmate_Main;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string openAiApiKey = builder.Configuration["OpenAiApiKey"];
        string openAiUrl = "https://api.openai.com/v1/chat/completions";

        builder.Services.AddHttpClient<IOpenAiService, OpenAiService>((httpClient, services) =>
        {
            httpClient.BaseAddress = new Uri(openAiUrl);           
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAiApiKey);
            return new OpenAiService(httpClient, services.GetRequiredService<ILogger<OpenAiService>>());
        });

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<UxCheckmateDbContext>(
                options => options
                .UseLazyLoadingProxies()    
                .UseSqlServer(
                    builder.Configuration.GetConnectionString("DBConnection")));

        var app = builder.Build();

        app.UseStatusCodePagesWithReExecute("/Home/ErrorPage");

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
