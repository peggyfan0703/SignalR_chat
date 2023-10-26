using Dapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using SignalR_chat.Controllers;
using SignalR_chat.Helper;
using SignalR_chat.Models;

namespace SignalRChat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IConfiguration _config;
        public ChatHub(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendMessage(string userid, string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);

            // 存入Redis
            MessageModel new_message = new MessageModel
            {
                userID = userid,
                userName = user,
                Content = message,
                Timestamp = System.DateTime.Now
            };
            List<MessageModel> messages = RedisHelper.Instance.Get<List<MessageModel>>("chatMessages");
            messages.Add(new_message);
            RedisHelper.Instance.Set<List<MessageModel>>("chatMessages", messages);


            // 將訊息入庫
            string result = string.Empty;
            string ConnectionString = _config["ConnectionStrings:DefaultConnection"];

            using (var conn = new MySqlConnection(ConnectionString))
            {
                var parameters = new
                {
                    userID = userid,
                    userName = user,
                    Content = message,
                    Timestamp = System.DateTime.Now
                };

                conn.Execute("INSERT INTO messages (userid, userName, Content, Timestamp) VALUE (@userid, @userName, @Content, @Timestamp)", parameters);

            }
        }
    }
}