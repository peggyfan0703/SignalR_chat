using Dapper;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using SignalR_chat.Models;
using StackExchange.Redis;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text;

namespace SignalR_chat.Helper
{
    public class RedisHelper
    {
        private static readonly string ConnectionString = "127.0.0.1:6379,password=123,ssl=False,abortConnect=False";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static object locker = new object();

        #region 連結Redis資料庫
        /// <summary>
        /// 連接
        /// </summary>
        private volatile IConnectionMultiplexer _connection;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IConnectionMultiplexer Connection
        {
            get
            {
                if (_connection != null && _connection.IsConnecting)
                    return _connection;
                lock (locker)
                {
                    if (_connection != null)
                    {
                        if (_connection.IsConnected)
                            return _connection;
                        else _connection.Dispose();
                    }
                    _connection = ConnectionMultiplexer.Connect(ConnectionString);
                }

                return _connection;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private volatile IDatabase _db;


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IDatabase Db
        {
            get
            {
                if (_db != null)
                    return _db;
                _db = Connection.GetDatabase();
                return _db;
            }
            set
            {
                _db = value;
            }
        }
        #endregion 連結Redis資料庫


        #region 實做Redis
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private RedisHelper()
        {
            _connection = ConnectionMultiplexer.Connect(ConnectionString);
            SetDatabase();
        }

        /// <summary>
        /// 
        /// </summary>
        private static RedisHelper redisHelper;

        /// <summary>
        /// 
        /// </summary>
        public static RedisHelper Instance
        {
            get
            {
                if(redisHelper == null)
                    lock(locker)
                    {
                        if(redisHelper == null)
                            redisHelper = new RedisHelper();
                    }
                return redisHelper;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db">要取得的資料庫ID</param>
        /// <returns></returns>
        public void SetDatabase(int? db = null) 
        {
            Db = Connection.GetDatabase(db ?? -1);
        }
        #endregion 實做Redis


        #region 新增、驗證存在、刪除

        /// <summary>
        /// 判斷該key是否存在
        /// </summary>
        /// <param name="key">關鍵字</param>
        /// <returns></returns>
        public bool IsExit(string key)
        {
            return Db.KeyExists(key);
        }

        /// <summary>
        /// 刪除redis
        /// </summary>
        /// <param name="key">關鍵字</param>
        /// <returns>存在時刪除；不存在返回False</returns>
        public virtual bool Delete(string key) 
        {
            if(IsExit(key))
                return Db.KeyDelete(key);
            return false;
        }

        /// <summary>
        /// 取得Redis 字串資料
        /// </summary>
        /// <param name="key">關鍵字</param>
        /// <returns></returns>
        public virtual string GetString(string key) 
        {
            return Db.StringGet(key);
        }


        /// <summary>
        /// 設定Redis 字串資料
        /// </summary>
        /// <param name="key">關鍵字</param>
        /// <param name="value">值</param>
        /// <param name="cacheTime">執行時間(分鐘)</param>
        public virtual bool SetString(string key, string value, int? cacheTime = null)
        {
            if (cacheTime != null)
                return Db.StringSet(key, value, TimeSpan.FromMinutes(Convert.ToDouble(cacheTime)));
            else
                return Db.StringSet(key, value);
        }

        /// <summary>
        /// 取得Redis 資料
        /// </summary>
        /// <param name="key">關鍵字</param>
        /// <returns></returns>
        public virtual T Get<T>(string key)
        {
            string value = GetString(key);
            if (string .IsNullOrEmpty(value))
                return default(T);
            else
                return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// 設定Redis 資料
        /// </summary>
        /// <param name="key">關鍵字</param>
        /// <param name="value">值</param>
        /// <param name="cacheTime">執行時間(分鐘)</param>
        public virtual void Set<T>(string key, T value, int? cacheTime = null)
        {
            if (value == null) return;

            string data = JsonConvert.SerializeObject(value);
            if (cacheTime != null)
                SetString(key, data, cacheTime);
            else
                SetString(key, data);
        }


        #endregion


        #region list使用

        /// <summary>
        /// 插入資料列
        /// </summary>
        /// <param name="key">關鍵字</param>
        /// <param name="value">值</param>
        /// <param name="IsTop">是否插入最上方列</param>
        /// <returns></returns>
        public virtual long ListPush<T>(string key, T value, bool IsTop = false)
        {
            if (value == null)
                return Db.ListLength(key);
            string data = JsonConvert.SerializeObject(value);
            if(IsTop)
                return Db.ListLeftPush(key, data);
            else
                return Db.ListRightPush(key,data);
        }

        /// <summary>
        /// 取得資料列
        /// </summary>
        /// <param name="key">關鍵字</param>
        /// <param name="IsTop">是否插入最上方列</param>
        /// <returns></returns>
        public virtual T ListPop<T>(string key, bool IsTop = false) 
        {
            string value = string.Empty;

            if(IsTop)
                value = Db.ListLeftPop(key);
            else
                value = Db.ListRightPop(key);

            if(value == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// 取得資料列長度
        /// </summary>
        /// <param name="key">關鍵字</param>
        /// <returns></returns>
        public virtual long GetListLength(string key)
        {
            return Db.ListLength(key);
        }

        #endregion


        #region 清空Redis資料
        /// <summary>
        /// 清空Redis資料
        /// </summary>
        public virtual void FlushDatabase(int db = -1)
        {
            using (ConnectionMultiplexer connection = ConnectionMultiplexer.Connect($"{ConnectionString},allowAdmin=true"))
            {
                IServer server = connection.GetServer(ConnectionString.Split(',')[0]);
                if (server != null)
                    server.FlushDatabase(db);
            }
        }

        #endregion 清空Redis資料










    }
}