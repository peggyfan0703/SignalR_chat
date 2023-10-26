using Microsoft.AspNetCore.SignalR;

namespace SignalRChat.Hubs
{
    public class InfoHub : Hub
    {
        public async Task OnConnectedAsync()
        {
            // 獲取ID
            string id = Context.ConnectionId;

            //string userID = HttpContext.Session.GetString("userID");


             
        }
    }
}