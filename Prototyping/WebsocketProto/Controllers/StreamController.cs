using System;
using System.Net.WebSockets;
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
        // GET api/values
        [HttpGet]
        public async Task Get()
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            if (isSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await GetMessages(context, webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        private async Task GetMessages(HttpContext context, WebSocket webSocket)
        {   //       
            string[] messages = new string[]
            {
                "Blog Post 1<br>Word: Quick, Count: 1<br>Word: Brown, Count: 2",
                "Blog Post 2<br>Word: Fox, Count: 3<br>Word: jumps, Count: 4",
                "Blog Post 3<br>Word: over, Count: 5<br>Word: the, Count: 6",
                "Blog Post 4<br>Word: Lazy, Count: 7<br>Word: Dog, Count: 8",

            };
            
            foreach (string message in messages)
            {
                var bytes = Encoding.ASCII.GetBytes(message);
                var arraySegment = new ArraySegment<byte>(bytes);
                await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                Thread.Sleep(2000); //sleeping so that we can see several messages are sent
            }

        }
    }
}
