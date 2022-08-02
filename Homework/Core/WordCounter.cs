using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Core
{
    public class WordCounter
    {
        private int lastResposneHash = 0;

        public bool areNewPostsAvailable(string rawWPApiResponse)
        {

            return false;
        }

        public int getHash(string rawWPApiResponse)
        {
            return rawWPApiResponse.GetHashCode();
        }

        public Dictionary<string, Dictionary<string, int>> getWordCountPerBlogPost(string rawWPApiResponse)
        {
            return new Dictionary<string, Dictionary<string, int>>();
        }

        public Dictionary<string, int> getWordCount(string cleanTextContent)
        {
            return new Dictionary<string, int>();
        }

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
    }
}
