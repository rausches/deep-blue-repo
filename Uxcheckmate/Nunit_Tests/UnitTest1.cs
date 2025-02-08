using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Uxcheckmate_Main.Models;

namespace Nunit_Tests;

// Testing server validation
public class UnitTest1
{
    [Test]
    public void ShouldFailWhenUrlEmpty()
    {
        // Arrange
        var model = new ReportUrl { Url = "" };
        var Validationcontext = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, Validationcontext, results, true);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].ErrorMessage, Is.EqualTo("The URL field is required."));
    }

    [Test]
    public void ShouldPassWhenUrlNotEmpty()
    {
        // Arrange
        var model = new ReportUrl { Url = "https://www.example.com" };
        var Validationcontext = new ValidationContext(model);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(model, Validationcontext, results, true);

        // Assert
        Assert.That(isValid, Is.True);    }
}