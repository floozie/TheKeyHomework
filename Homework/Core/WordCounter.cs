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
        private Core.WordpressApiResponseHelper wpApiResponseHelper = new Core.WordpressApiResponseHelper();
        private Dictionary<string, Dictionary<string, int>> wordCountDict = new Dictionary<string, Dictionary<string, int>>();
        private int lastResposneHash = 0;

        public bool areNewPostsAvailable(string responseHash)
        {

            int actualResponseHash = getResponseHash(responseHash);
            if (lastResposneHash != actualResponseHash)
            {
                lastResposneHash = actualResponseHash;
                return true;
            }
            return false;
        }

        public int getResponseHash(string rawWPApiResponse)
        {
            return rawWPApiResponse.GetHashCode();
        }

        public Dictionary<string, Dictionary<string, int>> getWordCountPerNewBlogPost(string json)
        {
            Dictionary<string, Dictionary<string, int>> newWordCountDict = new Dictionary<string, Dictionary<string, int>>();
            JArray jsonArray = JArray.Parse(json);
            foreach (JObject jsonObject in jsonArray)
            {
                string htmlEncodedTitle = jsonObject["title"]["rendered"].ToString();
                string cleanTextTitle = wpApiResponseHelper.getCleanText(htmlEncodedTitle);
                if (!wordCountDict.ContainsKey(cleanTextTitle) && !newWordCountDict.ContainsKey(cleanTextTitle))
                {
                    string htmlEncodedContent = jsonObject["content"]["rendered"].ToString();
                    string cleanTextContent = wpApiResponseHelper.getCleanText(htmlEncodedContent);
                    Dictionary<string, int> wordCounts = getWordCount(cleanTextContent);
                    newWordCountDict.Add(cleanTextTitle, wordCounts);
                    wordCountDict.Add(cleanTextTitle, wordCounts);
                }
            }
            return newWordCountDict;
        }

        public Dictionary<string, int> getWordCount(string cleanTextContent)
        {
            Dictionary<string, int> wordCountDict = new Dictionary<string, int>();
            string[] words = cleanTextContent.Split(' ');
            foreach (string word in words)
            {
                string lowerWord = word.Trim().ToLower();
                if (lowerWord.Length > 1) // German language has no words only one character long
                {
                    if (wordCountDict.ContainsKey(lowerWord))
                    {
                        wordCountDict[lowerWord]++;
                    }
                    else
                    {
                        wordCountDict.Add(lowerWord, 1);
                    }
                }
            }
            return wordCountDict;
        }
    }
}
