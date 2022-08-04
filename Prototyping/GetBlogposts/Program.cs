using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace GetBlogposts
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Getting Posts from www.internate.org");
            Console.WriteLine(decodeHtmlString("\u00fcber"));
            string request = "https://www.internate.org/wp-json/wp/v2/posts";
            string alllines = "";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(request);
            httpWebRequest.ContentType = "application/json";
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (
                var streamReader =
                    new StreamReader(httpResponse.GetResponseStream())
            )
            {
                string line = "";

                while ((line = streamReader.ReadLine()) != null)
                {
                    alllines += line;

                    //Console.WriteLine(line);
                }
            }
            string json = Program.getJsonFromMixedResponse(alllines);
            //Console.WriteLine (json);
            using (StreamWriter sw = new StreamWriter("jsonpayloadofromresponse.txt"))
            {

                sw.Write(json);


            }
            //JObject o = JObject.Parse(json);
            //var jsonObject = JObject.Parse(json);
            JArray jsonArray = JArray.Parse(json);
            Dictionary<string, Dictionary<string, int>> wordCountPerBlogPost = new Dictionary<string, Dictionary<string, int>>();
            foreach (JObject b in jsonArray)
            {

                /*
                 "title": {
"rendered": "Aktuelle Informationen zur Einreise nach Kanada"
},
"content": {
"rendered":*/
                Console.WriteLine("------------------------------------------");
                string htmlEncodedTitle = b["title"]["rendered"].ToString();
                string htmlDecodedTitle = removeHtmlTags(decodeHtmlString(htmlEncodedTitle));
                string emailAdressesandLinksRemovedTitle = removeLinks(removeEmailAddresses(htmlDecodedTitle));
                string specialCharactersRemovedTitle = removeSpecialCharacters(emailAdressesandLinksRemovedTitle);
                string cleanTextTitle = removeNumbers(specialCharactersRemovedTitle);
                Console.WriteLine(cleanTextTitle);
                if (!wordCountPerBlogPost.ContainsKey(cleanTextTitle))
                {
                    wordCountPerBlogPost.Add(cleanTextTitle, new Dictionary<string, int>());
                    Console.WriteLine("------------------------------------------");
                    string htmlEncodedContent = b["content"]["rendered"].ToString();
                    string htmlDecodedContent = removeHtmlTags(decodeHtmlString(htmlEncodedContent));
                    string emailAdressesandLinksRemovedContent = removeLinks(removeEmailAddresses(htmlDecodedContent));
                    string specialCharactersRemovedContent = removeSpecialCharacters(emailAdressesandLinksRemovedContent);
                    string cleanTextContent = removeNumbers(specialCharactersRemovedContent);
                    Console.WriteLine(cleanTextContent);

                    string[] words = cleanTextContent.Split(" ");

                    foreach (string word in words)
                    {
                        string lowerword = word.Trim().Replace("\n", "").ToLower();
                        if (lowerword.Length > 1) // German has no words only containing one character
                        {
                            if (wordCountPerBlogPost[cleanTextTitle].ContainsKey(lowerword))
                            {
                                wordCountPerBlogPost[cleanTextTitle][lowerword]++;
                            }
                            else
                            {
                                wordCountPerBlogPost[cleanTextTitle].Add(lowerword, 1);
                            }
                        }
                    }

                    foreach (string key in wordCountPerBlogPost[cleanTextTitle].Keys)
                    {
                        Console.WriteLine(key + ": " + wordCountPerBlogPost[cleanTextTitle][key]);
                    }

                }
                

                string input = Console.ReadLine();


            }

            /*foreach (JArray a in jsonArray)
                {
                    Console.WriteLine(a);
                }*/
        }

        public static string removeHtmlTags(string raw)
        {
            string pattern = @"<(.|\n)*?>";
            return Regex.Replace(raw, pattern, " ");
        }

        public static string removeLinks(string raw)
        {
            string pattern = @"http[^\s]+";
            return Regex.Replace(raw, pattern, " ");
        }

        public static string removeEmailAddresses(string raw)
        {
            string pattern = @"\S+@\S+(?:\.\S+)+";
            return Regex.Replace(raw, pattern, " ");
        }

        public static string removeSpecialCharacters(string raw)
        {
            string pattern = "\\p{P}";
            return Regex.Replace(raw, pattern, " ");
        }

        public static string removeNumbers(string raw)
        {
            string pattern = @"[\d-]";
            return Regex.Replace(raw, pattern, " ");
        }

        public static string decodeHtmlString(string raw)
        {
            StringWriter decodedStringWriter = new StringWriter();
            string nonbreakingspacesReplaced = Regex.Replace(raw, @"\u00A0", " ");
            HttpUtility.HtmlDecode(nonbreakingspacesReplaced, decodedStringWriter);
            return decodedStringWriter.ToString();
        }

        public static string getJsonFromMixedResponse(string response)
        {
            // TODO: find a smarter way to extract the json payload from the response string
            string jsonStartIndicator = "[{\"id\":";
            string jsonEndIndicator = "}]";
            int jsonStart = response.IndexOf(jsonStartIndicator);
            int jsonEnd = response.LastIndexOf(jsonEndIndicator);
            if (jsonEnd > -1 && jsonStart > -1)
            {
                string json = response.Substring(jsonStart, jsonEnd + jsonEndIndicator.Length - jsonStart);
                return json;
            }
            else
            {
                return "";
            }
        }
    }
}
