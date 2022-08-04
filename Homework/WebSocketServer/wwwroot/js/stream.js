var url = `ws://${location.host}/api/stream`
console.log('url is: ' + url);

var ulElement = document.getElementById('EventMessagePresenetationElement');
var webSocket = new WebSocket(url);
heartbeat();
webSocket.onmessage = function (message) {
    ulElement.innerHTML = ulElement.innerHTML += `<li>${message.data}</li>`
};
  
function heartbeat() {
    setTimeout(heartbeat, 1000);
    if (!webSocket) return;
    if (webSocket.readyState !== 1) return;
    webSocket.send('heartbeat');
    
  }

