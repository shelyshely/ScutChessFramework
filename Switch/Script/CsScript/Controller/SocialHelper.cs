using Switch.Script.CsScript.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Contract.ServerCom;
using ZyGames.Framework.RPC.IO;

namespace Switch.Script.CsScript.Controller
{
    /// <summary>
    /// 社交服辅助
    /// </summary>
    class SocialHelper
    {
        //获取社交服Session
        static private GameSession _socialSs = null;
        static public GameSession SocialSession
        {
            get
            {
                if (_socialSs == null) _socialSs = ServerSsMgr.Get((int)GlobalDefine.ServerIdType.SocialId);
                return _socialSs;
            }
            set { _socialSs = value; }
        }

        /// <summary>
        /// 是否存在社交服
        /// </summary>
        static public bool IsExistSocialS()
        {
            var socialSs = ServerSsMgr.Get((int)GlobalDefine.ServerIdType.SocialId);
            if (socialSs != null)
            {
                if (socialSs.IsHeartbeatTimeout)
                {
                    ServerSsMgr.DelSession(socialSs);
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
        /// 设置社交服
        /// </summary>
        public static void SetSocialS(GameSession ss)
        {
            //判断
            if (!ss.IsAuthorized || ss.UserId != (int)GlobalDefine.ServerIdType.SocialId)
            {
                TraceLog.WriteError("非法开启社交服");
                return;
            }
            ServerSsMgr.AddSession(ss);
            //
            TraceLog.WriteLine("社交服加入成功");
        }
        /// <summary>
        /// 社交服准备关闭
        /// </summary>
        static public void OnCloseSocialSBefore(GameSession session)
        {
            if (SocialSession != null && SocialSession.SessionId == session.SessionId)
            {
                Console.WriteLine("社交服准备关闭");
                ServerSsMgr.DelSession(session);
                SocialSession = null;
            }
        }

        /// <summary>
        /// 将目标Action跳转到社交服
        /// </summary>
        /// <param name="httpGet"></param>
        public static async void SendDesActionToSocialServer(HttpGet httpGet)
        {
            //检查
            if (SocialSession == null)
            {
                TraceLog.WriteError("SendDesActionToSocialServer error, 社交服没连接");
                return;
            }
            int desActionId = 0;
            if (!httpGet.GetInt("DesActionId", ref desActionId))
            {
                TraceLog.WriteError("SendDesActionToSocialServer error, 未指定目标ActionId");
                return;
            }
            //执行
            RequestParam param = new RequestParam();
            param["ActionId"] = desActionId;
            foreach (var key in httpGet.Keys)
            {
                if (key == "ActionId" || key == "DesActionId") continue;
                param[key] = httpGet[key];
            }
            string post = string.Format("?d={0}", System.Web.HttpUtility.UrlEncode(param.ToPostString()));
            var data = Encoding.UTF8.GetBytes(post);
            await SocialSession.SendAsync(data, 0, data.Length);
        }
    }
}
