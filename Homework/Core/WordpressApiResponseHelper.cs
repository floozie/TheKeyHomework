using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Text;
using System.IO;

namespace Core
{
    public class WordpressApiResponseHelper
    {
        public string extractJsonFromResponseWPResponse(string response)
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

        public string removeHtmlTags(string raw)
        {
            string pattern = @"<(.|\n)*?>";
            return Regex.Replace(raw, pattern, string.Empty);
        }

        public string removeLinks(string raw)
        {
            string pattern = @"http[^\s]+";
            return Regex.Replace(raw, pattern, string.Empty);
        }

        public string removeEmailAddresses(string raw)
        {
            string pattern = @"\S+@\S+(?:\.\S+)+";
            return Regex.Replace(raw, pattern, string.Empty);
        }

        public string removeSpecialCharacters(string raw)
        {
            string pattern = "\\p{P}";
            return Regex.Replace(raw, pattern, " ");
        }

        public string removeNumbers(string raw)
        {
            string pattern = @"[\d-]";
            return Regex.Replace(raw, pattern, " ");
        }

        public string decodeHtmlString(string raw)
        {
            StringWriter decodedStringWriter = new StringWriter();
            HttpUtility.HtmlDecode(raw, decodedStringWriter);
            return decodedStringWriter.ToString();
        }
    }
}
