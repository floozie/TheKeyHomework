using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json.Linq;

namespace GetBlogposts
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Getting Posts from www.internate.org");
            string request = "https://www.internate.org/wp-json/wp/v2/posts";
            string alllines = "";
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(request);
            httpWebRequest.ContentType = "application/json";

            //httpWebRequest.Accept = "*/*";
            // httpWebRequest.Method = "GET";
            //httpWebRequest.Headers.Add("Authorization", "Basic reallylongstring");
            var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();

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
                string json = Program.getJsonFromMixedResponse(alllines);
                Console.WriteLine (json);
                //JObject o = JObject.Parse(json);
                //var jsonObject = JObject.Parse(json);
                JArray jsonArray = JArray.Parse(json);
                foreach (JObject a in jsonArray)
                {
                    Console.WriteLine(a);
                }
            }

            /*foreach (JArray a in jsonArray)
                {
                    Console.WriteLine(a);
                }*/
        }

        public static string getJsonFromMixedResponse(string mixedString)
        {
            string jsonstartindicator = "[{\"id\":";
            string jsonendindicator = "}]";

            int jsonstart = mixedString.IndexOf(jsonstartindicator);
            int jsonend = mixedString.LastIndexOf(jsonendindicator);
            string json =
                mixedString
                    .Substring(jsonstart,
                    jsonend + jsonendindicator.Length - jsonstart);
            return json;
        }
    }
}
