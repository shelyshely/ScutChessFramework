using Switch.Script.CsScript.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Contract.ServerCom;

namespace Switch.Script.CsScript.Controller
{
    /// <summary>
    /// 挖豹子服辅助
    /// </summary>
    class WbzServerHelper
    {
        //获取挖豹子服Session
        static private GameSession _wabaoziSs = null;
        static public GameSession WabaoziSession
        {
            get
            {
                if (_wabaoziSs == null) _wabaoziSs = ServerSsMgr.Get((int)GlobalDefine.ServerIdType.WabaoziId);
                return _wabaoziSs;
            }
            set { _wabaoziSs = value; }
        }

        /// <summary>
        /// 是否存在挖豹子服
        /// </summary>
        static public bool IsExistWabaoziS()
        {
            var wabaoziSs = WabaoziSession;
            if (wabaoziSs != null)
            {
                if (wabaoziSs.IsHeartbeatTimeout)
                {
                    ServerSsMgr.DelSession(wabaoziSs);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 设置挖豹子服
        /// </summary>
        public static void SetWabaoziS(GameSession ss)
        {
            //判断
            if (!ss.IsAuthorized || ss.UserId != (int)GlobalDefine.ServerIdType.WabaoziId)
            {
                TraceLog.WriteError("非法开启挖豹子服");
                return;
            }
            ServerSsMgr.AddSession(ss);
            //
            TraceLog.WriteLine("挖豹子服加入成功");
        }
        /// <summary>
        /// 挖豹子服准备关闭
        /// </summary>
        static public void OnCloseWabaoziSBefore(GameSession session)
        {
            if (WabaoziSession != null && WabaoziSession.SessionId == session.SessionId)
            {
                Console.WriteLine("挖豹子服准备关闭");
                ServerSsMgr.DelSession(session);
                WabaoziSession = null;
            }
        }
    }
}
