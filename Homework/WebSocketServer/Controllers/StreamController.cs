using System;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace MyApplication.Controllers
{
    [Route("api/[controller]")]
    public class StreamController : Controller
    {
        private TimeSpan connectionTimeoutTime = new TimeSpan(0, 0, 5);
        private System.DateTime lastHeartbeatReceived;
        // GET api/values
        [HttpGet]
        public async Task Get()
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            if (isSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                ReceiveHeartbeats(context, webSocket);
                await SendMessages(context, webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
        private async Task ReceiveHeartbeats(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            while (true)
            {
                try
                {

                    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), new CancellationTokenSource(5000).Token);
                    if (System.Text.Encoding.Default.GetString(buffer).Contains("heartbeat"))
                    {
                        lastHeartbeatReceived = System.DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                    break;
                }

            }
        }



        private async Task SendMessages(HttpContext context, WebSocket webSocket)
        {

            lastHeartbeatReceived = System.DateTime.Now;

            string requestUri = "https://www.internate.org/wp-json/wp/v2/posts";
            Dictionary<string, Dictionary<string, int>> wordCountPerNewBlogPost = new Dictionary<string, Dictionary<string, int>>();
            Core.WordpressApiResponseHelper wpApiResponseHelper = new Core.WordpressApiResponseHelper();
            Core.WordCounter wordCounter = new Core.WordCounter();
            //Console.WriteLine(receiveResult.CloseStatus);
            while (isConnected())
            {

                string rawResponseFromWPApi = wpApiResponseHelper.getRawResponseFromWPApi(requestUri);
                string json = wpApiResponseHelper.extractJsonFromResponseWPResponse(rawResponseFromWPApi);
                if (wordCounter.areNewPostsAvailable(json))
                {
                    Console.WriteLine("New posts available -> pushing messages to the frontend");
                    wordCountPerNewBlogPost = wordCounter.getWordCountPerNewBlogPost(json);

                    foreach (string cleanTextTitle in wordCountPerNewBlogPost.Keys)
                    {
                        var bytes = Encoding.ASCII.GetBytes("<h2>" + wpApiResponseHelper.encodeHtmlString(cleanTextTitle) + "</h2>");
                        var arraySegment = new ArraySegment<byte>(bytes);
                        await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                        foreach (string key in wordCountPerNewBlogPost[cleanTextTitle].Keys)
                        {
                            bytes = Encoding.ASCII.GetBytes(wpApiResponseHelper.encodeHtmlString(key) + ": " + wordCountPerNewBlogPost[cleanTextTitle][key]);
                            arraySegment = new ArraySegment<byte>(bytes);
                            await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                }

                System.Threading.Thread.Sleep(5000);
            }
        }

        private bool isConnected()
        {

            if (System.DateTime.Now - lastHeartbeatReceived > connectionTimeoutTime)
            {
                Console.WriteLine("Client connection lost -> stopping polling wordpress api");
                return false;
            }

            return true;
        }
    }
}

