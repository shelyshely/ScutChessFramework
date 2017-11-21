using ServiceStack.Net30.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZyGames.Framework.Game.Contract.ServerCom
{
    /// <summary>
    /// 各服Session管理类
    /// </summary>
    public class ServerSsMgr
    {
        private static GameSession _lobbySession;
        private static GameSession _switchSession;

        private static ConcurrentDictionary<string/*server sid*/, GameSession> _serverSessions;
        private static ConcurrentDictionary<int/*serverUserId*/, string/*server sid*/> _serverUserHash;

        static ServerSsMgr()
        {
            _lobbySession = null;
            _serverSessions = new ConcurrentDictionary<string, GameSession>();
            _serverUserHash = new ConcurrentDictionary<int, string>();
        }

        //////////////////////////////////////////////////////////////////////////////////////////
        //大厅服
        public static GameSession GetLobbySession()
        {
            if (_lobbySession != null) return _lobbySession;
            return null;
        }

        public static void SetLobbySession(GameSession ss)
        {
            _lobbySession = ss;
        }
        //////////////////////////////////////////////////////////////////////////////////////////
        //路由服
        public static GameSession GetSwitchSession()
        {
            if (_switchSession != null) return _switchSession;
            return null;
        }

        public static void SetSwitchSession(GameSession ss)
        {
            _switchSession = ss;
        }
        //////////////////////////////////////////////////////////////////////////////////////////
        public static GameSession Get(string sid)
        {
            GameSession session;
            return _serverSessions.TryGetValue(sid, out session) ? session : null;
        }

        public static GameSession Get(int userId)
        {
            string ssid;
            return _serverUserHash.TryGetValue(userId, out ssid) ? Get(ssid) : null;
        }

        public static void AddSession(GameSession ss)
        {
            if (ss != null && !_serverSessions.ContainsKey(ss.SessionId))
            {
                //设置心跳超时时间2分钟
                ss.CustomHeartbeatTimeout = 120;
                _serverSessions[ss.SessionId] = ss;
                //
                if (ss.UserId > 0) _serverUserHash[ss.UserId] = ss.SessionId;
            }
        }

        public static void DelSession(GameSession ss)
        {
            if (ss != null && _serverSessions.ContainsKey(ss.SessionId))
            {
                var userId = ss.UserId;
                GameSession session;
                _serverSessions.TryRemove(ss.SessionId, out session);
                if (userId > 0)
                {
                    string ssid;
                    _serverUserHash.TryRemove(userId, out ssid);
                }
            }
        }
    }
}
