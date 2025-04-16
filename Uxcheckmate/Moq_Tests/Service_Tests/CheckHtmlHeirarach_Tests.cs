using NUnit.Framework;
using Uxcheckmate_Main.Models;
using System.Collections.Generic;

namespace Service_Tests
{
    public class CheckHtmlHierarchy_Tests
    {
        private CheckHtmlHierarchy _checker;

        [SetUp]
        public void Setup()
        {
            _checker = new CheckHtmlHierarchy();
        }
        [Test]
        // No html should have no issues
        public void NoHeaders_NoProblems()
        {
            _checker.html = "";
            Assert.That(_checker.problemsFound(), Is.False);
            Assert.That(_checker.ProblemSpots().Count, Is.EqualTo(0));
        }
        [Test]
        // Proper hierarchy going from h1, h2, and h3
        public void ProperHierarchy_NoProblems()
        {
            _checker.html = "<h1>Main Title</h1><h2>Sub Title</h2><h3>Section Title</h3>";
            Assert.That(_checker.problemsFound(), Is.False);
            Assert.That(_checker.ProblemSpots().Count, Is.EqualTo(0));
        }
        [Test]
        // Settings first header to not equal 1
        public void FirstHeadingNotH1_HasProblem()
        {
            _checker.html = "<h2>Title</h2><h3>Subtitle</h3>";
            Assert.That(_checker.problemsFound(), Is.True);
            List<string> problems = _checker.ProblemSpots();
            Assert.That(problems.Count, Is.EqualTo(1));
            Assert.That(problems[0], Does.Contain("The first heading is h2"));
        }
        [Test]
        // Making sure headers don't jump in values
        public void JumpInHierarchy_HasProblems()
        {
            _checker.html = "<h1>Main Title</h1><h3>Section Title</h3>";
            Assert.That(_checker.problemsFound(), Is.True);
            List<string> problems = _checker.ProblemSpots();
            Assert.That(problems.Count, Is.EqualTo(1));
            Assert.That(problems[0], Does.Contain("Heading jump between"));
            Assert.That(problems[0], Does.Contain("Expected heading level h2 but found h3"));
        }
        [Test]
        // Creating multiple issues, first heading not h1 and a skip
        public void MultipleIssues_HasMultipleProblems()
        {
            _checker.html = "<h3>Title</h3><h2>Sub Title</h2><h4>Section Title</h4>";
            Assert.That(_checker.problemsFound(), Is.True);
            List<string> problems = _checker.ProblemSpots();
            Assert.That(problems[0], Does.Contain("The first heading is h3"));
        }
    }
}
