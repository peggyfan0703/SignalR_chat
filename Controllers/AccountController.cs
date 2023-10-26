using Microsoft.AspNetCore.Mvc;
using SignalR_chat.Models;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Reflection.Metadata;
using Dapper;
using System.Security.Cryptography;
using System.Data.Common;
using MySql.Data.MySqlClient;
using static Org.BouncyCastle.Crypto.Digests.SkeinEngine;
using System.Data;
using System.Collections.Generic;

namespace SignalR_chat.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;

        public AccountController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UserModel model)
        {
            string ConnectionString = _config["ConnectionStrings:DefaultConnection"];
            string strSQL = @"INSERT INTO users (userID, userName, password) 
                            VALUE (@userID, @userName, @password);
                            ";

            using (MySqlConnection conn = new MySqlConnection(ConnectionString))
            {
                int execute = conn.Execute(
                                "INSERT INTO users (userID, userName, password) VALUES (@userID, @userName, @password)",
                                model
                            );

                var test = "1";





                ////設定參數
                //DynamicParameters parameters = new DynamicParameters();
                //parameters.Add("@Param1", "abc", DbType.String, ParameterDirection.Input);
                //parameters.Add("@Return1", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                //conn.Execute("MyStoredProcedure", parameters, commandType: CommandType.StoredProcedure);
                //int result2 = parameters.Get<int>("@Return1");

                //cn.Open();
                //using (SqlCommand cmd = new SqlCommand(strSQL, cn))
                //{
                //    cmd.Parameters.Add("@userID", System.Data.SqlDbType.VarChar).Value = model.userID;
                //    cmd.Parameters.Add("@userName", System.Data.SqlDbType.VarChar).Value = model.userName;
                //    cmd.Parameters.Add("@password", System.Data.SqlDbType.VarChar).Value = model.password;
                //    ////1.回傳DataSet or DataTable 
                //    //using (SqlDataAdapter adpter = new SqlDataAdapter(cmd))
                //    //{
                //    //    System.Data.DataSet ds = new System.Data.DataSet();
                //    //    adpter.Fill(ds);
                //    //    System.Data.DataTable dt = ds.Tables[0];

                //    //    foreach (System.Data.DataRow dr in dt.Rows)
                //    //    {
                //    //        ObjFgUserData user = getPersonFromDataRow(dr);
                //    //        users.Add(user);//DataRow Mapping To Model
                //    //    }
                //    //}
                //}
            }
            //return users;

            // 返回登入頁面
            return RedirectToAction("index", "Login");
        }

    }
}