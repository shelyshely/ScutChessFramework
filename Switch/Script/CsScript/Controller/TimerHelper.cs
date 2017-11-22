using System;
using System.Diagnostics;
using System.IO;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Common.Configuration;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Runtime;
using ZyGames.Framework.Redis;
using ZyGames.Framework.Script;
namespace Switch.Script.CsScript.Controller
{
    /// <summary>
    /// 定时器辅助器
    /// </summary>
    class TimerHelper
    {
        private const int _timer1sKey = 1;
        private static int _secCnt = 0;

        /// <summary>
        /// Redis dump.rdb路径
        /// </summary>
        private static string _redisRdbPath = string.Empty;

        static TimerHelper()
        {
            //Redis dump.rdb文件监测
            var path = ConfigUtils.GetSetting("RedisRdbPath", string.Empty);
            if(!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    _redisRdbPath = path;
                }
                else
                {
                    DataSyncQueueManager.BgSave();
                    if (File.Exists(path))
                    {
                        _redisRdbPath = path;
                    }
                }
            }
            if (string.IsNullOrEmpty(_redisRdbPath)) TraceLog.WriteError("Redis dump.rdb 文件未在监测中...");
            else
            {
                DataSyncQueueManager.BgSave();
                Console.WriteLine("Redis {0} 文件成功监测中...", _redisRdbPath);
            }
        }

        /// <summary>
        /// 启动定时器
        /// </summary>
        public static void StartTimer() 
        {
            var mainClass = ScriptEngines.GetCurrentMainScript();
            if (mainClass != null)
            {
                if (((dynamic)mainClass)._dictTimer.ContainsKey(_timer1sKey))
                {
                    ((dynamic)mainClass)._dictTimer[_timer1sKey].Stop();
                    ((dynamic)mainClass)._dictTimer[_timer1sKey].Dispose();
                    //
                    Console.WriteLine("定时器释放...");
                }
                var timer = new System.Timers.Timer();
                timer.Enabled = true;
                timer.Interval = 1 * 1000;
                timer.Start();
                timer.Elapsed += new System.Timers.ElapsedEventHandler(TimeDo_1s);
                ((dynamic)mainClass)._dictTimer[_timer1sKey] = timer;
                //
                Console.WriteLine("定时器启动成功...");
            }
        }

        /// <summary>
        /// Redis rdb是否正常
        /// </summary>
        /// <returns></returns>
        public static bool isRedisRdbWork()
        {
            if (!string.IsNullOrEmpty(_redisRdbPath))
            {
                var fi = new System.IO.FileInfo(_redisRdbPath);
                var totalSecs = (DateTime.Now - fi.LastWriteTime).TotalSeconds;
                if (totalSecs > 600)
                {
                    return false; 
                }
                else if (totalSecs >= 300)
                {
                    DataSyncQueueManager.BgSave();
                }
            }

            return true;
        }

        //定时器
        private static void TimeDo_1s(object sender, System.Timers.ElapsedEventArgs e)
        {
            _secCnt++;
            int intHour = e.SignalTime.Hour;
            int intMinute = e.SignalTime.Minute;
            int intSecond = e.SignalTime.Second;

            //Redis检查
            if ((_secCnt + 1) % 5 == 0)
            {
                var isEffect = RedisConnectionPool.CheckConnect();
                if (GameEnvironment.IsRedisReady != isEffect)
                {
                    if (isEffect) isEffect = isRedisRdbWork();
                    GameEnvironment.IsRedisReady = isEffect;
                }
            }
            if((_secCnt+2) % 30 == 0)
            {
                if(!isRedisRdbWork())
                {
                    GameEnvironment.IsRedisReady = false;
                    TraceLog.WriteError("Redis dump.rdb 文件长时间未被修改...");
                }
            }
            if ((_secCnt + 3) % 300 == 0)
            {
                //监听端口参数信息
                var mainClass = ZyGames.Framework.Script.ScriptEngines.GetCurrentMainScript();
                if (mainClass != null)
                {
                    Console.WriteLine("监听端口参数信息:" + ((dynamic)mainClass).GetSocketHostInfo());
                }
            }
        }
    }
}
