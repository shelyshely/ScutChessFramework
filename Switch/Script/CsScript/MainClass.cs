using Switch.Script.CsScript.Action;
using Switch.Script.CsScript.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Contract.SwitchServer;
using ZyGames.Framework.Game.Message;
using ZyGames.Framework.Game.Runtime;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.Net;
using ZyGames.Framework.Redis;
using ZyGames.Framework.RPC.IO;
using ZyGames.Framework.RPC.Sockets;
using ZyGames.Framework.Script;

namespace Switch.Script.CsScript
{
    public class MainClass : SwitchSocketHost, IMainScript
    {

        public Dictionary<int, System.Timers.Timer> _dictTimer = new Dictionary<int, System.Timers.Timer>();

        /// <summary>
        /// 异常缓存文件
        /// </summary>
        private string _entitySetExFile = "entitySetExFile.txt";

        private Guid _guid = Guid.NewGuid();

        public MainClass()
        {
        }


        /// <summary>
        /// 获取监听端口参数信息
        /// </summary>
        /// <returns></returns>
        public string GetSocketHostInfo()
        {
            var strList = new List<string>();
            strList.Add(string.Format("acceptEventArgsPoolSize:{0}", this.GetAcceptEventArgsPoolSize()));
            strList.Add(string.Format("ioEventArgsPoolSize:{0}", this.GetIoEventArgsPoolSize()));
            return string.Join(" | ", strList);
        }

        /// <summary>
        /// 执行action
        /// </summary>
        /// <param name="actionType"></param>
        private void SendAction(string actionType, object message = null)
        {
            var actionId = 90016;
            var session = GameSession.Get(_guid);
            if (session == null) session = GameSession.CreateNew(_guid, new HttpRequest("", "http://127.0.0.1", ""));
            var str1 = DateTime.Now.GetHashCode().ToString();
            var str2 = ZyGames.Framework.Common.Security.CryptoHelper.MD5_Encrypt(str1 + GMHelper._wordkey, Encoding.UTF8);
            Parameters param = new Parameters();
            param.Add("Word", str1 + ";" + str2);
            param.Add("ActionType", actionType);
            RequestPackage package = ActionFactory.GetResponsePackage(actionId, session, param as Parameters, OpCode.Binary, message);
            IActionDispatcher actionDispatcher = new ScutActionDispatcher();
            ActionFactory.GetActionResponse(actionDispatcher, actionId, session, package);

        }

        /// <summary>
        /// 
        /// </summary>
        public override void ReStart()
        {
            GameEnvironment.IsRunning = true;
        }

        protected override void OnStartAffer()
        {
            try
            {
                RequestParam.SignKey = GameEnvironment.Setting.ProductSignKey;
                //redis异常缓存载入
                if (File.Exists(_entitySetExFile))
                {
                    byte[] data = File.ReadAllBytes(_entitySetExFile);
                    if (DataSyncQueueManager.ProEntityExWait(data))
                    {
                        File.Delete(_entitySetExFile);
                    }
                    else
                    {
                        throw new Exception("异常缓存载入失败");
                    }
                }

                SendAction("OnStartAffer");
                ScriptEngines.OnLoaded += OnScriptLoaded;
                GameSession.OnRecover += OnSessionRecover;
            }
            catch (Exception ex)
            {
                TraceLog.WriteError("OnStartAffer error:{0}", ex);
            }
        }

        protected override void OnServiceStop()
        {
            GameEnvironment.Stop();
            //程序退出前redis异常缓存数据需备份
            if(!RedisConnectionPool.CheckConnect())
            { 
                if(DataSyncQueueManager._entitySetExWaitList.Count > 0)
                {
                    File.WriteAllBytes(_entitySetExFile, DataSyncQueueManager.GetEntityExWaitData());
                }
            }
        }
        // 判断客户端发起的心跳包(30s发一次)是否有中断发送，需要设置GameSession.HeartbeatTimeout属性，当超过HeartbeatTimeout值还未收到客户端的心跳包，说明玩家已经下线，此方法就会被调用。
        protected override void OnHeartbeatTimeout(GameSession session)
        {
            //这里处理未收到close指令的断线业务
            //Console.WriteLine("客户端IP:{0} is heartbeat timeout.", session.RemoteAddress);
            OnDisconnected(session);
        }

        protected override void OnConnectCompleted(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine("客户端IP:[{0}]已与服务器连接成功", e.Socket.RemoteEndPoint);
            base.OnConnectCompleted(sender, e);
        }
        //当客户端发起close指令时，服务端收到后会触发调用此方法，可以及时处理玩家断线，但不包括未发close指令的。
        protected override void OnDisconnected(GameSession session)
        {
            //这里处理收到close指令的断线业务
            if (session != null && session.IsAuthorized)
            {
                SendAction("OnClientDisconnected", session);
            }
            //
            base.OnDisconnected(session);
        }

        ///-------------------------------
        protected override void OnRequested(ActionGetter actionGetter, BaseGameResponse response)
        {
            //Console.WriteLine("请求Action:{0},{1}", actionGetter.GetActionId(), actionGetter.RemoteAddress);
            base.OnRequested(actionGetter, response);
        }

        protected override void OnHeartbeat(GameSession session)
        {
            //Console.WriteLine("{0}>>Hearbeat package: {1} session count {2}", DateTime.Now.ToString("HH:mm:ss"), session.RemoteAddress, GameSession.Count);
            base.OnHeartbeat(session);
        }

        //动态编译后处理
        private void OnScriptLoaded(string type, string[] files)
        {
            SendAction("OnScriptLoaded");
        }

        /// <summary>
        /// Session恢复时处理
        /// </summary>
        private void OnSessionRecover(Object sender, EventArgs e)
        {
            if(sender is GameSession)
            {
                GameSession ss = (GameSession)sender;
                SendAction("OnSessionRecover", ss);
            }
        }
    }
}
