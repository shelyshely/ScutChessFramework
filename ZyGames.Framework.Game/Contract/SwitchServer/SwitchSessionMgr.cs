using ServiceStack.Net30.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZyGames.Framework.Common;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Contract.ServerCom;

namespace ZyGames.Framework.Game.Contract.SwitchServer
{
    /// <summary>
    /// 客户端远程代理数据
    /// </summary>
    public class ProxIdData
    {
        public string serverSid;        //连接服sessionId
        public DateTime activeTime;     //活跃时间(过期删除)
    }

    /// <summary>
    /// 路由Session管理
    /// </summary>
    public class SwitchSessionMgr
    {
        private static ConcurrentDictionary<string/*proxId*/, ProxIdData> _globalProxIdData;

        private static Timer _clearTime;
        public static int _timeout;

        static SwitchSessionMgr()
        {
            _globalProxIdData = new ConcurrentDictionary<string, ProxIdData>();
            //
            _timeout = 2 * 60 * 60;//2h
            _clearTime = new Timer(OnClearSession, null, 60000, 60000);
        }
        //////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 获取客户端代理数据
        /// </summary>
        public static ProxIdData GetProxIdData(string proxId)
        {
            ProxIdData proxIdData = null;
            _globalProxIdData.TryGetValue(proxId, out proxIdData);
            if(proxIdData != null)
            {
                return proxIdData;
            }
            return null;
        }

        /// <summary>
        /// 增加或更新客户端代理数据
        /// </summary>
        public static void AddOrUpdateProxIdData(string proxId, string serverSid)
        {
            var proxIdData = GetProxIdData(proxId);
            if(proxIdData != null)
            {
                if (proxIdData.serverSid != serverSid) proxIdData.serverSid = serverSid;
                proxIdData.activeTime = DateTime.Now;
            }
            else
            {
                _globalProxIdData[proxId] = new ProxIdData() { serverSid = serverSid, activeTime = DateTime.Now, };
            }
        }

        /// <summary>
        /// 获取所有在线客户端代理proxId
        /// </summary>
        /// <returns></returns>
        public static List<string> GetOnlineAll(int delayTime = 120)
        {
            var t = MathUtils.Now.AddSeconds(-delayTime);
            List<string> list = new List<string>();
            foreach (var pair in _globalProxIdData)
            {
                var proxIdData = pair.Value;
                if (proxIdData.activeTime >= t)
                {
                    list.Add(pair.Key);
                }
            }
            return list;
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        public static GameSession GetConnectSession(string proxId)
        {
            var proxIdData = GetProxIdData(proxId);
            if (proxIdData == null) return null;
            return ServerSsMgr.Get(proxIdData.serverSid);
        }
        //////////////////////////////////////////////////////////////////////////////////////////
        private static void OnClearSession(object state)
        {
            try
            {
                foreach (var pair in _globalProxIdData)
                {
                    var proxIdData = pair.Value;
                    if (proxIdData.activeTime < MathUtils.Now.AddSeconds(-_timeout))
                    {
                        TraceLog.WriteInfo("proxId{0} is expire {1}({2}sec)", pair.Key, proxIdData.activeTime, _timeout);
                        ProxIdData old;
                        _globalProxIdData.TryRemove(pair.Key, out old);
                    }
                }
            }
            catch (Exception er)
            {
                TraceLog.WriteError("SwitchSessionMgr OnClearSession error:{0}", er);
            }
        }
    }
}
