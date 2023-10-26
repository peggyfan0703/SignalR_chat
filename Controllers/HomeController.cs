using Dapper;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SignalR_chat.Models;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using SignalR_chat.Helper;

namespace SignalR_chat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

      
        public IActionResult Index(string userName)
        {
            ViewBag.user = userName;
            return View();
        }


        /// <summary>
        /// 留言版
        /// </summary>
        /// <returns></returns>
        public ActionResult Chat(string userID)
        {

            // 記錄登入者資訊
            ViewBag.userID = RedisHelper.Instance.GetString("userID_" + userID);
            ViewBag.userName = RedisHelper.Instance.GetString("userName_" + userID);

            // 取得聊天訊息
            GetRedis("chatMessages"); //排程開起後可以不用讀取
            ViewData.Model = RedisHelper.Instance.Get<List<MessageModel>>("chatMessages");

            return View();
        }


        /// <summary>
        /// 讀取資料庫資料後存入Redis
        /// </summary>
        private void GetRedis(string key)
        {
            // chatMessages     
            string ConnectionString = _config["ConnectionStrings:DefaultConnection"];

            using (var conn = new MySqlConnection(ConnectionString))
            {
                string strSql = "SELECT userID, userName, Content, Timestamp  FROM chat.messages WHERE date_sub(curdate(),interval 7 day)<=date(Timestamp) ORDER BY timestamp";
                List<MessageModel> messages = conn.Query<MessageModel>(strSql).ToList();
                RedisHelper.Instance.Set<List<MessageModel>>(key, messages);
                
            }
        }

    }
}