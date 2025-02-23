namespace Nunit_Tests;
using System.IO;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;
public class Database_Tests
{
    private UxCheckmateDbContext _context;
    [SetUp]
    public void Setup()
    {
        var configPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "appsettings.json");
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.WorkDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var dbConnectionString = configuration.GetConnectionString("DBConnection");

        // Debugging output
        Console.WriteLine($"Retrieved Connection String: {dbConnectionString}");

        if (string.IsNullOrEmpty(dbConnectionString))
        {
            throw new InvalidOperationException("DBConnection was not found in appsettings.json!");
        }

        var options = new DbContextOptionsBuilder<UxCheckmateDbContext>()
            .UseSqlServer(dbConnectionString)  // Directly use the string, not relying on named connections
            .Options;

        _context = new UxCheckmateDbContext(options);

    }

    [Test]
    public void Database_Connected()
    {
        bool canConnect = _context.Database.CanConnect();
        Assert.That(canConnect, Is.True);
    }

    [Test]
    public void Read_Categories()
    {
        var DesignCategory = _context.DesignCategories.ToList();
        Assert.That(DesignCategory, Is.Not.Empty);
        Assert.That(DesignCategory.Any(d => d.Id == 1 && d.Name == "Visual Hierarchy"), Is.True);
        Assert.That(DesignCategory.Any(d => d.Id == 1 && d.Name == "Broken Links"), Is.True);
    }

    [Test]
    public void Add_Test_Issue_Test()
    {
        var category = new DesignIssue { CategoryId = 1, ReportId = 1, Message = "Test Message", Severity = 1 };
        _context.DesignIssues.Add(category);
        _context.SaveChanges();
        var testCategory = _context.DesignIssues.FirstOrDefault(c => c.Message == "Test Message");
        Assert.That(testCategory, Is.Not.Null);
        Assert.That(testCategory.CategoryId, Is.EqualTo(1));
        Assert.That(testCategory.ReportId, Is.EqualTo(1));
        Assert.That(testCategory.Severity, Is.EqualTo(1));
    }

    [TearDown]
    public void TearDown()
    {
        var testCategories = _context.DesignIssues.Where(c => c.Message == "Test Message");
        _context.DesignIssues.RemoveRange(testCategories);
        _context.SaveChanges();
        _context.Dispose();
    }
}
