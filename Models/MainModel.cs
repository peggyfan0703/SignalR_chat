using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SignalR_chat.Models
{
    public class MainModel
    {
    }
    public class UserModel
    {
        public string userID { get; set; }
        public string password { get; set; }
        public string userName { get; set; }
    }
    public class MessageModel
    {
        public string userID { get; set; }
        public string userName { get; set; }
        public string Content {  get; set; }
        public DateTime Timestamp { get; set; }        
    }
}
