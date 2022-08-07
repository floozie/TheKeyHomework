using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Core;
using System.IO;

namespace Core_Unittest
{
    public class CoreTester
    {
        private Core.WordpressApiResponseHelper wordpressApiResponseHelper = new Core.WordpressApiResponseHelper();
        private Core.WordCounter wordCounter = new Core.WordCounter();

        private readonly ITestOutputHelper output;

        public CoreTester(ITestOutputHelper output)
        {
            this.output = output;
        }

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
                Assert.Equal(assertData, wordpressApiResponseHelper.extractJsonFromResponseWPResponse(assertData));

            }

        }

        [Fact]
        public void compareHashCodesTest()
        {

            string inputData1Filename = "WPApiResponse2Assert.txt";
            string inputData2Filename = "WPApiResponse1Assert.txt";
            string inputData1 = getDataFromFile(inputData1Filename);
            string inputData2 = getDataFromFile(inputData2Filename);
            // same hashes
            Assert.Equal(wordCounter.getResponseHash(inputData1), wordCounter.getResponseHash(inputData1));
            // hashes differ
            Assert.NotEqual(wordCounter.getResponseHash(inputData1), wordCounter.getResponseHash(inputData2));
        }


        [Fact]
        public void decodeHtmlTest()
        {
            string decoded = wordpressApiResponseHelper.decodeHtmlString("\u00fcber");
            string compare = "über";
            Assert.Equal(decoded, compare);

            decoded = wordpressApiResponseHelper.decodeHtmlString("Quarant\u00e4nebestimmungen nach. Die kanadische Regierung stellt bei technischen Problemen\u00a0");
            compare = "Quarantänebestimmungen nach. Die kanadische Regierung stellt bei technischen Problemen ";
            Assert.Equal(decoded, compare);

            decoded = wordpressApiResponseHelper.decodeHtmlString("Aktuelle Informationen zur Einreise nach Gro\u00dfbritannien");
            compare = "Aktuelle Informationen zur Einreise nach Großbritannien";
            Assert.Equal(decoded, compare);

            decoded = wordpressApiResponseHelper.decodeHtmlString(" Zeit g\u00f6nnen wir");
            compare = " Zeit gönnen wir";
            Assert.Equal(decoded, compare);        

        }

        [Fact]
        public void removeHtmlTagsTest()
        {
            string decoded = wordpressApiResponseHelper.removeHtmlTags("<strong>weitere Informationen</strong></a>\u00a0bereit.</p>");
            string compare = " weitere Informationen  \u00a0bereit. ";
            Assert.Equal(decoded, compare);

            decoded = wordpressApiResponseHelper.removeHtmlTags("<span style=\"font-size: 16px;\">\u00a0</span>deutsche Sch\u00fcler ganz unterschiedlich</h1>");
            compare = " \u00a0 deutsche Sch\u00fcler ganz unterschiedlich ";
            Assert.Equal(decoded, compare);
        }


        [Fact]
        public void removeEmailAdressesTest()
        {
            string decoded = wordpressApiResponseHelper.removeEmailAddresses("das ist eine gelöschte email adresse: peter.pan@peterpan.de peter pan");
            string compare = "das ist eine gelöschte email adresse:   peter pan";
            Assert.Equal(decoded, compare);

            decoded = wordpressApiResponseHelper.removeEmailAddresses("das ist eine gelöschte email adresse: peter.pan-überflieger@peterpan.de peter pan");
            compare = "das ist eine gelöschte email adresse:   peter pan";
            Assert.Equal(decoded, compare);

        }

        [Fact]
        public void removeLinksTest()
        {
            string decoded = wordpressApiResponseHelper.removeLinks("das ist ein gelöschter link: http://www.peter.pan peter pan");
            string compare = "das ist ein gelöschter link:   peter pan";
            Assert.Equal(decoded, compare);

            decoded = wordpressApiResponseHelper.removeLinks("das ist ein gelöschter link: https://www.peter.pan peter pan");
            compare = "das ist ein gelöschter link:   peter pan";
            Assert.Equal(decoded, compare);

        }


        [Fact]
        public void removeSpecialCharactersTest()
        {
            string decoded = wordpressApiResponseHelper.removeSpecialCharacters("In diesem 'satz', gab(gibt) es, jede Menge SonderZeichen?!?\"");
            string compare = "In diesem  satz   gab gibt  es  jede Menge SonderZeichen    ";
            Assert.Equal(decoded, compare);

            decoded = wordpressApiResponseHelper.removeSpecialCharacters("das ist ein gelöschter link: https://www.peter.pan peter pan");
            compare = "das ist ein gelöschter link  https   www peter pan peter pan";
            Assert.Equal(decoded, compare);

        }

        [Fact]
        public void removeNumbersTest()
        {
            string decoded = wordpressApiResponseHelper.removeNumbers("Seit dem 15. Februar 2021");
            string compare = "Seit dem   . Februar     ";
            Assert.Equal(decoded, compare);

            decoded = wordpressApiResponseHelper.removeNumbers("Telefon 0611 180 58 80 oder Email");
            compare = "Telefon                oder Email";
            Assert.Equal(decoded, compare);

        }

        [Fact]
        public void getCleanTextTest()
        {
            string decoded = wordpressApiResponseHelper.getCleanText("in vielen F\u00e4llen weiterhin Pr\u00e4senzunterricht anzubieten und die sozialen Kontakte aufrechtzuerhalten: 0611 180 58 80");
            string compare = "in vielen Fällen weiterhin Präsenzunterricht anzubieten und die sozialen Kontakte aufrechtzuerhalten                ";
            Assert.Equal(decoded, compare);

            decoded = wordpressApiResponseHelper.getCleanText("Ab dem 8. M\u00e4rz soll f\u00fcr alle Sch\u00fcler wieder der Pr\u00e4senzunterricht beginnen");
            compare = "Ab dem    März soll für alle Schüler wieder der Präsenzunterricht beginnen";
            Assert.Equal(decoded, compare);

            decoded = wordpressApiResponseHelper.getCleanText("<strong><a href=\"https://www.germany.info/us-de/service/corona/2313816\">deutschen Botschaft in den USA</a></strong>, sowie dem <a href=\"https://www.cdc.gov/quarantine/fr-proof-negative-test.html\"><strong>Center of Disease Control \u2013 CDC</strong></a>.</p>\n<p>Bef\u00f6rderungsbedingungen der jeweiligen Fluggesellschaften sollten dringend beachtet werden.</p>\n<p>Eine umfangreiche Hilfestellung zum Thema Covid-19 und Visum finden Sie auf der Seite der <a href=\"https://de.usembassy.gov/de/haeufig-gestellte-fragen-zu-covid-19-visa/\">US-Botschaft");
            compare = "  deutschen Botschaft in den USA    sowie dem   Center of Disease Control   CDC      Beförderungsbedingungen der jeweiligen Fluggesellschaften sollten dringend beachtet werden    Eine umfangreiche Hilfestellung zum Thema Covid    und Visum finden Sie auf der Seite der  US Botschaft";
            Assert.Equal(decoded, compare);

        }

        [Fact]
        public void getWordCountTest()
        {
            Core.WordCounter wordCounter = new Core.WordCounter();
            Dictionary<string, int> wordcountData = wordCounter.getWordCount("eins zwei drei zwei drei drei a b c dreiminuszwei");
            Dictionary<string, int> wordcountExpectedData = new Dictionary<string, int>();
            wordcountExpectedData.Add("eins",1);
            wordcountExpectedData.Add("zwei",2);
            wordcountExpectedData.Add("drei",3);
            wordcountExpectedData.Add("dreiminuszwei",1);
            Assert.Equal(wordcountData, wordcountExpectedData);

            wordcountData = wordCounter.getWordCount("Eins Zwei drei zwei Drei drei a B c Dreiminuszwei");
            Assert.Equal(wordcountData, wordcountExpectedData);
        }

        [Fact]
        public void getWordCountPerNewBlogPostTest()
        {
            Core.WordCounter wordCounter = new Core.WordCounter();
            Dictionary<string, Dictionary<string, int>> wordCountPerNewBlogPostData = wordCounter.getWordCountPerNewBlogPost("[{\"id\":16216,\"title\":{\"rendered\":\"Aktuelle Informationen zur Einreise nach Kanada\"},\"content\":{\"rendered\":\"<h1><strong>Aktuelle Informationen zur Einreise nach Kanada</strong></h1>\n<p>Die f\u00fcr Kanada grunds\u00e4tzlich bestehende Einreisesperre wurde f\u00fcr vollst\u00e4ndig Geimpfte aufgehoben.\"}}]");
            Dictionary<string, int> wordcountExpectedData = new Dictionary<string, int>();
            wordcountExpectedData.Add("aktuelle",1);
            wordcountExpectedData.Add("informationen",1);
            wordcountExpectedData.Add("zur",1);
            wordcountExpectedData.Add("einreise",1);
            wordcountExpectedData.Add("nach",1);
            wordcountExpectedData.Add("kanada",2);
            wordcountExpectedData.Add("die",1);
            wordcountExpectedData.Add("für",2);
            wordcountExpectedData.Add("grundsätzlich",1);
            wordcountExpectedData.Add("bestehende",1);
            wordcountExpectedData.Add("einreisesperre",1);
            wordcountExpectedData.Add("wurde",1);
            wordcountExpectedData.Add("vollständig",1);
            wordcountExpectedData.Add("geimpfte",1);
            wordcountExpectedData.Add("aufgehoben",1);
            Assert.Equal(wordcountExpectedData, wordCountPerNewBlogPostData["Aktuelle Informationen zur Einreise nach Kanada"]);
        }

        [Fact]
        public void simulateOnlySendingNewPostData()
        {
            // As there is no write access to the wordpress page, the functionality of "new posts will result a frontendupdate" cant be tested.
            // To be pretty confident that the functionality works it uses this simulation scenario 
            string jsonFileContainingOnePlogPost = "WPApiResponse3Assert.txt";
            string jsonfileContainingAllPlogPosts = "WPApiResponse1Assert.txt";
            string jsonDataContainingOneBlogPost = getDataFromFile(jsonFileContainingOnePlogPost); 
            string jsonDataContainingAllBlogPosts = getDataFromFile(jsonfileContainingAllPlogPosts); 
            
            // Here its checking if the json contains new posts compared to the internal wordcounter data
            // then it parses the json containing one blogpost into the wordcounter data, returns the word count map that should contain one blogpost dictionary
            Core.WordCounter wordCounter = new Core.WordCounter();
            Assert.True(wordCounter.areNewPostsAvailable(jsonFileContainingOnePlogPost));
            Dictionary<string, Dictionary<string, int>> wordCountPerNewBlogPostDataSingle = wordCounter.getWordCountPerNewBlogPost(jsonDataContainingOneBlogPost);
            Assert.Single(wordCountPerNewBlogPostDataSingle);

            // Then it checks that the same json data does not contain new information -> no update of frontend would be needed
            // Nevertheless calling  getWordCountPerNewBlogPost for the same wordCounter object should not return data
            Assert.False(wordCounter.areNewPostsAvailable(jsonFileContainingOnePlogPost));
            Dictionary<string, Dictionary<string, int>> wordCountPerNewBlogPostDataEmpty = wordCounter.getWordCountPerNewBlogPost(jsonDataContainingOneBlogPost);
            Assert.Empty(wordCountPerNewBlogPostDataEmpty);
            
            // Now its simulating a need to update frontend scenario as new posts are available. Json with full 10 posts is passed as an argument
            // It will return the data of new posts. Its 10 posts in total, as already one has been consumed it only returns the 9 new blogposts as the result 
            Assert.True(wordCounter.areNewPostsAvailable(jsonDataContainingAllBlogPosts));
            Dictionary<string, Dictionary<string, int>> wordCountPerNewBlogPostDataAllButSingle = wordCounter.getWordCountPerNewBlogPost(jsonDataContainingAllBlogPosts);
            Assert.Equal(9,wordCountPerNewBlogPostDataAllButSingle.Count);

            // Probe: create a fresh wordCounetr object and again json with full 10 posts is passed as an argument
            // This should now return all 10 posts. 1 post result + (full posts result after 1 post result on same wordcounter object) = full posts result with fresh wordCounter object 
            Core.WordCounter freshwordCounter = new Core.WordCounter();

            Assert.True(freshwordCounter.areNewPostsAvailable(jsonDataContainingAllBlogPosts));
            Dictionary<string, Dictionary<string, int>> wordCountPerNewBlogPostDataAll = freshwordCounter.getWordCountPerNewBlogPost(jsonDataContainingAllBlogPosts);
            Assert.Equal(wordCountPerNewBlogPostDataSingle.Count+wordCountPerNewBlogPostDataAllButSingle.Count,wordCountPerNewBlogPostDataAll.Count);

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

        private void writeDataToFile(string filename, string data)
        {
            using (StreamWriter sw = new StreamWriter("..\\..\\..\\TestData\\" + filename))
            {
                sw.Write(data);
            }
        }



    }
}
