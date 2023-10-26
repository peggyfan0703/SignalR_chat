using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using SignalR_chat.Models;
using System.Diagnostics;
using SignalR_chat.Helper;

namespace SignalR_chat.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public LoginController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult Index(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                // 帳號密碼是空值。
                //var errors = ModelState.Values.SelectMany(v => v.Errors);
                //foreach (var error in errors)
                //{
                //    // 輸出錯誤訊息 測試用
                //    Console.WriteLine(error.ErrorMessage);
                //}
                //Create book.
                ModelState.Clear();
                ModelState.AddModelError(string.Empty, "帳號或密碼不可為空。");
            }
            else
            {
                // 帳號密碼不是空值。

                // 驗證帳號密碼
                string userName = IsValidUser(model.userID, model.password);
                if (userName != String.Empty)
                {
                    RedisHelper.Instance.SetString("userID_"+ model.userID, model.userID);
                    RedisHelper.Instance.SetString("userName_" + model.userID, userName);

                    var routeValues = new RouteValueDictionary
                    {
                        { "userID", model.userID }
                    };

                    return RedirectToAction("chat", "Home" , routeValues) ;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "帳號或密碼錯誤。");
                }
            }

            return View();
        }

        // 驗證帳號密碼
        private string IsValidUser(string userID, string password)
        {
            string result = string.Empty;
            string ConnectionString = _config["ConnectionStrings:DefaultConnection"];

            using (var conn = new MySqlConnection(ConnectionString))
            {
                var parameters = new
                {
                    userID = userID,
                    password = password
                };

                var sql = conn.QueryFirstOrDefault<string>("SELECT userName FROM users WHERE userID = @userID AND password = @password", parameters);
                if (sql != null)
                {
                    result = sql;
                }

            }
            return result;
        }

        // 歡迎頁
        public IActionResult Welcome()
        {
            return View();
        }

    }
}