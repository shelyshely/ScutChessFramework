using Switch.Script.CsScript.Defines;
using Switch.Script.Model.DataModel;
using Switch.Script.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Common;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Contract.ServerCom;
using ZyGames.Framework.Game.Contract.SwitchServer;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.RPC.IO;

namespace Switch.Script.CsScript.Controller
{
    /// <summary>
    /// 服务器辅助器
    /// 专门处理与其它服的交互
    /// </summary>
    public class ServerHelper
    {
        static private ShareCacheStruct<SConnectServer> sConnectServerSet = new ShareCacheStruct<SConnectServer>();
        static private ShareCacheStruct<SRoomServer> sRoomServerSet = new ShareCacheStruct<SRoomServer>();

        public static void Init() 
        {
            foreach (var item in sConnectServerSet.FindAll(false)) sConnectServerSet.Delete(item);
            foreach (var item in sRoomServerSet.FindAll(false)) sRoomServerSet.Delete(item);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////连接服//////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 获取连接服数量
        /// </summary>
        static public int GetConnectServerCnt() { return sConnectServerSet.FindAll(false).Count; }

        /// <summary>
        /// 获取空闲的连接服的UserId
        /// </summary>
        static public int GetFreeCSUserId()
        {
            var list = sConnectServerSet.FindAll(false).Select(t => t.UserId).ToList();
            for (var i = GlobalDefine.CS_UserId_Min; i < GlobalDefine.CS_UserId_Max; ++i)
            {
                if (!list.Exists(m => m == i))
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// 是否存在连接服
        /// </summary>
        static public bool IsExistCS(int _serverId) 
        {
            var connectServer = sConnectServerSet.FindKey(_serverId);
            if(connectServer != null)
            {
                var connectSs = ServerSsMgr.Get(connectServer.CSSid);
                if (connectSs != null)
                {
                    if (connectSs.IsHeartbeatTimeout)
                    {
                        ServerSsMgr.DelSession(connectSs);
                        sConnectServerSet.Delete(connectServer);
                        connectServer = null;
                    }
                }
            }

            return connectServer != null; 
        }

        /// <summary>
        /// 增加连接服
        /// </summary>
        static public void AddCS(int serverId, GameSession ss)
        {
            //判断
            if (!ss.IsAuthorized)
            {
                TraceLog.WriteError("非法开启连接服，ServerId:{0}", serverId);
                return;
            }
            //加入
            var sConnectServer = new SConnectServer() { ServerId = serverId, Status = ServerStatus.Connected, CSSid = ss.SessionId, UserId = ss.UserId };
            sConnectServerSet.Add(sConnectServer);

            ServerSsMgr.AddSession(ss);
            //
            TraceLog.WriteLine("连接服加入成功，ServerId:{0},剩余：{1}", serverId, sConnectServerSet.FindAll(false).Count);
        }
        /// <summary>
        /// 连接服准备关闭
        /// </summary>
        static public void OnCloseCSBefore(GameSession session)
        {
            var serverInfo = sConnectServerSet.Find(m => m.CSSid == session.SessionId);
            if (serverInfo != null)
            {
                Console.WriteLine("连接服准备关闭，ServerId:{0}", serverInfo.ServerId);
                serverInfo.Status = ServerStatus.Closeing;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////房间服/////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 获取房间服数量
        /// </summary>
        static public int GetRoomServerCnt() { return sRoomServerSet.FindAll(false).Count; }

        /// <summary>
        /// 获取空闲的房间服的UserId
        /// </summary>
        static public int GetFreeRSUserId()
        {
            var list = sRoomServerSet.FindAll(false).Select(t => t.UserId).ToList();
            for (var i = GlobalDefine.RS_UserId_Min; i < GlobalDefine.RS_UserId_Max; ++i)
            {
                if (!list.Exists(m => m == i))
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// 是否存在房间服
        /// </summary>
        static public bool IsExistRS(int _serverId) 
        { 
            var roomServer = sRoomServerSet.FindKey(_serverId);
            if (roomServer != null)
            {
                var roomSs = ServerSsMgr.Get(roomServer.RSSid);
                if (roomSs != null)
                {
                    if (roomSs.IsHeartbeatTimeout)
                    {
                        ServerSsMgr.DelSession(roomSs);
                        sRoomServerSet.Delete(roomServer);
                        roomServer = null;
                    }
                }
            }

            return roomServer != null; 
        }

        /// <summary>
        /// 增加房间服
        /// </summary>
        static public void AddRS(int serverId, GameSession ss)
        {
            //判断
            if (!ss.IsAuthorized)
            {
                TraceLog.WriteError("非法开启房间服，ServerId:{0}", serverId);
                return;
            }
            //加入
            var sRoomServer = new SRoomServer() { ServerId = serverId, Status = ServerStatus.Connected, RSSid = ss.SessionId, UserId = ss.UserId };
            sRoomServerSet.Add(sRoomServer);

            ServerSsMgr.AddSession(ss);
            //
            TraceLog.WriteLine("房间服加入成功，ServerId:{0},剩余：{1}", serverId, sRoomServerSet.FindAll(false).Count);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////大厅服/////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 是否存在大厅服
        /// </summary>
        static public bool IsExistLS() 
        {
            var lobbySs = ServerSsMgr.GetLobbySession();
            if(lobbySs != null)
            {
                if(lobbySs.IsHeartbeatTimeout)
                {
                    ServerSsMgr.SetLobbySession(null);
                    ServerSsMgr.DelSession(lobbySs);
                }
            }
            return ServerSsMgr.GetLobbySession() != null; 
        }

        /// <summary>
        /// 设置大厅服
        /// </summary>
        public static void SetLS(int serverId, GameSession ss)
        {
            //判断
            if (!ss.IsAuthorized)
            {
                TraceLog.WriteError("非法开启大厅服，ServerId:{0}", serverId);
                return;
            }
            ServerSsMgr.SetLobbySession(ss);
            ServerSsMgr.AddSession(ss);
            //
            TraceLog.WriteLine("大厅服加入成功");
        }
        /// <summary>
        /// 大厅服准备关闭
        /// </summary>
        static public void OnCloseLSBefore(GameSession session)
        {
            var lobbySs = ServerSsMgr.GetLobbySession();
            if (lobbySs != null && lobbySs.SessionId == session.SessionId)
            {
                Console.WriteLine("大厅服断开连接");
                ServerSsMgr.SetLobbySession(null);
                ServerSsMgr.DelSession(session);
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////其它///////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 其它服断开处理
        /// </summary>
        /// <param name="session"></param>
        public static void OnOtherServerDisconnected(GameSession session)
        {
            string serverSid = session.SessionId;
            do
            {
                var serverInfo = sConnectServerSet.Find(m => m.CSSid == serverSid);
                if (serverInfo != null)
                {
                    Console.WriteLine("连接服断开连接，ServerId:{0},剩余：{1}", serverInfo.ServerId, sConnectServerSet.FindAll(false).Count - 1);
                    sConnectServerSet.Delete(serverInfo);
                    break;
                }
                if (ServerSsMgr.GetLobbySession() != null &&
                    ServerSsMgr.GetLobbySession().SessionId == session.SessionId)
                {
                    Console.WriteLine("大厅服断开连接");
                    ServerSsMgr.SetLobbySession(null);
                    break;
                }
                if(session.UserId == GlobalDefine.ServerIdType.SocialId.ToInt())
                {
                    Console.WriteLine("社交服断开连接");
                    SocialHelper.SocialSession = null;
                    break;
                }
                if (session.UserId == GlobalDefine.ServerIdType.SuperMonreyId.ToInt())
                {
                    Console.WriteLine("拉霸机服断开连接");
                    SuperMonreyHelper.SuperMonreySession = null;
                    break;
                }
                if (session.UserId == GlobalDefine.ServerIdType.WabaoziId.ToInt())
                {
                    Console.WriteLine("挖豹子服断开连接");
                    WbzServerHelper.WabaoziSession = null;
                    break;
                }
                var rsInfo = sRoomServerSet.Find(m => m.RSSid == serverSid);
                if (rsInfo != null)
                {
                    Console.WriteLine("房间服断开连接，ServerId:{0},剩余：{1}", rsInfo.ServerId, sRoomServerSet.FindAll(false).Count - 1);
                    sRoomServerSet.Delete(rsInfo);
                    break;
                }
            } while (false);
            //
            ServerSsMgr.DelSession(session);
        }
    }
}
