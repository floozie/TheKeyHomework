using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Text;
using System.IO;
using System.Net;

namespace Core
{
    public class WordpressApiResponseHelper
    {
        public string getRawResponseFromWPApi(string requestUri)
        {
            string completeResponse = "";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUri);
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
                    completeResponse += line;
                }
            }
            return completeResponse;
        }

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

        public string getCleanText(string raw)
        {
            string htmlDecoded = removeHtmlTags(decodeHtmlString(raw));
            string emailAdressesandLinksRemoved = removeLinks(removeEmailAddresses(htmlDecoded));
            string specialCharactersRemoved = removeSpecialCharacters(emailAdressesandLinksRemoved);
            string cleanText = removeNumbers(specialCharactersRemoved);
            return cleanText.Replace("\n"," ");
        }

        public string removeHtmlTags(string raw)
        {
            string pattern = @"<(.|\n)*?>";
            return Regex.Replace(raw, pattern, " ");
        }

        public string removeLinks(string raw)
        {
            string pattern = @"http[^\s]+";
            return Regex.Replace(raw, pattern, " ");
        }

        public string removeEmailAddresses(string raw)
        {
            string pattern = @"\S+@\S+(?:\.\S+)+";
            return Regex.Replace(raw, pattern, " ");
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
            string nonbreakingspacesReplaced = Regex.Replace(raw, @"\u00A0", " ");
            HttpUtility.HtmlDecode(nonbreakingspacesReplaced, decodedStringWriter);
            return decodedStringWriter.ToString();
        }

        public string encodeHtmlString(string decoded)
        {
            return HttpUtility.HtmlEncode(decoded);
        }
    }
}
