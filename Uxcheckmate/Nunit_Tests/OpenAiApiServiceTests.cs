using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using Uxcheckmate_Main.Services;

namespace Uxcheckmate_Tests.Services
{
    [TestFixture]
    public class OpenAiServiceUnitTests
    {
        private OpenAiService _openAiService;

        [SetUp]
        public void Setup()
        {
            _openAiService = new OpenAiService(null, null); 
        }


        /* Test if FormatScrapedData function returns a formatted string */
        [Test]
        public void FormatScrapedData_ReturnFormattedString()
        {
        }

        /* Test if ExtractSections function returns a dictionary */
        [Test]
        public void ExtractSections_ReturnDictionary()
        {
        }

        /* Tests if  */
        [Test]
        public void ConvertToUxResult_ReturnsUxResult()
        {
        }
    }
}
