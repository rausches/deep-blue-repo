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
        //Inside Nunit_Tests folder make sure you have the txt file dbconfig.txt with information like below
        //Data Source=localhost,1433;Initial Catalog=uxcheckmate;User Id=YourUsername;Password='YourPassword';TrustServerCertificate=True;
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "runPa11y.js");
        var configuration = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(configPath)!).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
        var DbConnection = configuration.GetConnectionString("DBConnection");
        var options = new DbContextOptionsBuilder<UxCheckmateDbContext>().UseSqlServer(DbConnection).Options;
        _context = new UxCheckmateDbContext(options);
    }

    [Test]
    public void Database_Connected()
    {
        bool canConnect = _context.Database.CanConnect();
        Assert.That(canConnect, Is.True);
    }

    [Test]
    public void Read_Roles()
    {
        var DesignCategory = _context.DesignCategories.ToList();
        Assert.That(DesignCategory, Is.Not.Empty);
        Assert.That(DesignCategory.Any(d => d.Id == 1 && d.Name == "Visual Hierarchy"), Is.True);
        Assert.That(DesignCategory.Any(d => d.Id == 2 && d.Name == "Broken Links"), Is.True);
    }

    [Test]
    public void Add_Test_Category_Test()
    {
        var category = new ReportCategory { Name = "Test Category", Description = "Test description", OpenAiprompt = "Test prompt" };
        _context.ReportCategories.Add(category);
        _context.SaveChanges();
        var testCategory = _context.ReportCategories.FirstOrDefault(c => c.Name == "Test Category");
        Assert.That(testCategory, Is.Not.Null);
        Assert.That(testCategory.Description, Is.EqualTo("Test description"));
        Assert.That(testCategory.OpenAiprompt, Is.EqualTo("Test prompt"));
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }

    [TearDown]
    public void TearDown()
    {
        var testCategories = _context.ReportCategories.Where(c => c.Name == "Test Category");
        _context.ReportCategories.RemoveRange(testCategories);
        _context.SaveChanges();
        _context.Dispose();
    }
}
