using System;
using Xunit;
using Core;
using System.IO;

namespace Core_Unittest
{
    public class CoreTester
    {
        [Fact]
        public void extractJsonFromResponseTest()
        {
            // WPApiEmptyResponse.txt - Response Data of: https://www.internate.org/wp-json/wp/v2/posts?after=2021-07-20T15:10:46
            // WPApiResponse1.txt     - Response Data of: https://www.internate.org/wp-json/wp/v2/posts
            // WPApiResponse2.txt     - Response Data of: https://www.internate.org/wp-json/wp/v2/posts?after=2021-06-20T15:10:46
            // WPApiResponse3.txt     - Response Data of: https://www.internate.org/wp-json/wp/v2/posts?after=2021-07-20T15:10:45
            string[] tests = new string[] { "WPApiEmptyResponse", "WPApiResponse1", "WPApiResponse2", "WPApiResponse3" };
            foreach (string test in tests)
            {
                string inputDataFilename = test + ".txt";
                string assertDataFilename = test + "Assert.txt";
                string inputData = getDataFromFile(inputDataFilename);
                string assertData = getDataFromFile(assertDataFilename);
                Assert.Equal(assertData, new Core.WordpressApiResponseHelper().extractJsonFromResponseWPResponse(assertData));

            }

        }

        [Fact]
        public void compareHashCodesTest()
        {
            string inputDataFilename = "WPApiResponse1.txt";
            string assertDataFilename = "WPApiResponse2.txt";
            string inputData = getDataFromFile(inputDataFilename);
            string assertData = getDataFromFile(assertDataFilename);
            Assert.Equal(new Core.WordCounter().getHash(inputData), new Core.WordCounter().getHash(inputData));
            Assert.NotEqual(new Core.WordCounter().getHash(inputData), new Core.WordCounter().getHash(assertData));
        }


        private string getDataFromFile(string filename)
        {
            string data;
            using (StreamReader sr = new StreamReader("..\\..\\..\\TestData\\" + filename))
            {
                data = sr.ReadToEnd();
            }
            return data;
        }

    }
}
