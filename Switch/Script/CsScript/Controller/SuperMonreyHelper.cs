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
    /// 拉霸机服辅助
    /// </summary>
    class SuperMonreyHelper
    {
        //获取拉霸机服Session
        static private GameSession _superMonreySs = null;
        static public GameSession SuperMonreySession
        {
            get
            {
                if (_superMonreySs == null) _superMonreySs = ServerSsMgr.Get((int)GlobalDefine.ServerIdType.SuperMonreyId);
                return _superMonreySs;
            }
            set { _superMonreySs = value; }
        }

        /// <summary>
        /// 是否存在拉霸机服
        /// </summary>
        static public bool IsExistSuperMonreyS()
        {
            var superMonreySs = ServerSsMgr.Get((int)GlobalDefine.ServerIdType.SuperMonreyId);
            if (superMonreySs != null)
            {
                if (superMonreySs.IsHeartbeatTimeout)
                {
                    ServerSsMgr.DelSession(superMonreySs);
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
        /// 设置拉霸机服
        /// </summary>
        public static void SetSuperMonreyS(GameSession ss)
        {
            //判断
            if (!ss.IsAuthorized || ss.UserId != (int)GlobalDefine.ServerIdType.SuperMonreyId)
            {
                TraceLog.WriteError("非法开启拉霸机服");
                return;
            }
            ServerSsMgr.AddSession(ss);
            //
            TraceLog.WriteLine("拉霸机服加入成功");
        }
        /// <summary>
        /// 拉霸机服准备关闭
        /// </summary>
        static public void OnCloseSuperMonreySBefore(GameSession session)
        {
            if (SuperMonreySession != null && SuperMonreySession.SessionId == session.SessionId)
            {
                Console.WriteLine("拉霸机服准备关闭");
                ServerSsMgr.DelSession(session);
                SuperMonreySession = null;
            }
        }
    }
}
