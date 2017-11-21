/****************************************************************************
Copyright (c) 2013-2015 scutgame.com
社交服Socket类
说明：用于连接大厅服的Socket
****************************************************************************/
using System;
using System.IO;
using System.Net;
using System.Threading;
using ZyGames.Framework.Common.Configuration;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Config;
using ZyGames.Framework.Game.Lang;
using ZyGames.Framework.Game.Runtime;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.RPC.Sockets;
using ZyGames.Framework.RPC.IO;
using System.Text;
using System.Web;

namespace ZyGames.Framework.Game.Contract.ServerCom
{
    /// <summary>
    /// 与大厅服通讯Socket
    /// </summary>
    public abstract class LobbySocketClient : GameBaseHost
    {
        //private SmartThreadPool threadPool;

        private const int BufferSize = 1024;
        private Timer _timer;
        private ClientSocket _socketRoom;

        /// <summary>
        /// Heartbeat packet data.
        /// </summary>
        public byte[] HeartPacket { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Connected
        {
            get { return _socketRoom.Connected; }
            private set { _socketRoom.Connected = value; }
        }

        /// <summary>
        /// Connect
        /// </summary>
        public void Connect()
        {
            _socketRoom.Connect();
        }
        /// <summary>
        /// Close
        /// </summary>
        public void Close()
        {
            _socketRoom.Close();
        }

        /// <summary>
        /// Action repeater
        /// </summary>
        public IActionDispatcher ActionDispatcher
        {
            get { return _setting == null ? null : _setting.ActionDispatcher; }
            set
            {
                if (_setting != null)
                {
                    _setting.ActionDispatcher = value;
                }
            }
        }

        private EnvironmentSetting _setting;

        protected LobbySocketClient()
        {

        }
        protected void Init(string host, int port, int heartInterval)
        {
            _setting = GameEnvironment.Setting;
            var remoteEndPoint = new IPEndPoint(Dns.GetHostAddresses(host)[0], port);
            var settings = new ClientSocketSettings(BufferSize, remoteEndPoint);
            _socketRoom = new ClientSocket(settings);
            _socketRoom.DataReceived += new SocketEventHandler(roomSocket_DataReceived);
            _socketRoom.Disconnected += new SocketEventHandler(roomSocket_Disconnected);
            RequestParam heartParam = new RequestParam();
            heartParam["ActionId"] = (int)ActionEnum.Heartbeat;
            heartParam["MsgId"] = 0;
            string post = string.Format("?d={0}", HttpUtility.UrlEncode(heartParam.ToPostString()));
            HeartPacket = Encoding.ASCII.GetBytes(post);
            _timer = new Timer(DoCheckHeartbeat, null, 1000, heartInterval);
        }

        private void roomSocket_Disconnected(object sender, SocketEventArgs e)
        {
            try
            {
                if (e.Socket == null) return;
                GameSession session = GameSession.Get(e.Socket.HashCode);
                if (session != null)
                {
                    OnDisconnected(session);
                    session.ProxySid = Guid.Empty;
                    session.Close();
                }
            }
            catch (Exception err)
            {
                TraceLog.WriteError("Disconnected error:{0}", err);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public async System.Threading.Tasks.Task Send(byte[] data)
        {
            if (!Connected)
            {
                Connect();
            }
            await _socketRoom.PostSend(data, 0, data.Length);
        }

        private void roomSocket_DataReceived(object sender, SocketEventArgs e)
        {
            try
            {
                OnReceivedBefore(e);
                RequestPackage package;
                if (!ActionDispatcher.TryDecodePackage(e, out package))
                {
                    return;
                }
                var session = GetSession(e, package);
                if (CheckSpecialPackge(package, session))
                {
                    return;
                }
                package.Bind(session);
                ProcessPackage(package, session);
            }
            catch (Exception ex)
            {
                TraceLog.WriteError("Received to Host:{0} error:{1}", e.Socket.RemoteEndPoint, ex);
            }
        }


        private GameSession GetSession(SocketEventArgs e, RequestPackage package)
        {
            //使用代理分发器时,每个ssid建立一个游服Serssion
            GameSession session;
            if (package.ProxySid != Guid.Empty)
            {
                session = GameSession.Get(package.ProxySid) ??
                          (package.IsProxyRequest
                              ? GameSession.Get(e.Socket.HashCode)
                              : GameSession.CreateNew(package.ProxySid, e.Socket, _socketRoom));
                if (session != null)
                {
                    session.ProxySid = package.ProxySid;
                }
            }
            else
            {
                session = GameSession.Get(package.SessionId) ?? GameSession.Get(e.Socket.HashCode);
            }
            if (session == null)
            {
                session = GameSession.CreateNew(e.Socket.HashCode, e.Socket, _socketRoom);
            }
            if ((!session.Connected || !Equals(session.RemoteAddress, e.Socket.RemoteEndPoint.ToString())))
            {
                GameSession.Recover(session, e.Socket.HashCode, e.Socket, _socketRoom);
            }
            return session;
        }

        /// <summary>
        /// Raises the received before event.
        /// </summary>
        /// <param name="e">E.</param>
        protected virtual void OnReceivedBefore(SocketEventArgs e)
        {
        }

        private async System.Threading.Tasks.Task ProcessPackage(RequestPackage package, GameSession session)
        {
            if (package == null) return;

            try
            {
                ActionGetter actionGetter;
                byte[] data = new byte[0];
                if (!string.IsNullOrEmpty(package.RouteName))
                {
                    actionGetter = ActionDispatcher.GetActionGetter(package, session);
                    if (CheckRemote(package.RouteName, actionGetter))
                    {
                        MessageStructure response = new MessageStructure();
                        OnCallRemote(package.RouteName, actionGetter, response);
                        data = response.PopBuffer();
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    SocketGameResponse response = new SocketGameResponse();
                    response.WriteErrorCallback += ActionDispatcher.ResponseError;
                    actionGetter = ActionDispatcher.GetActionGetter(package, session);
                    DoAction(actionGetter, response);
                    data = response.ReadByte();
                }
                try
                {
                    if (session != null && data.Length > 0)
                    {
                        byte userRet = 0;
                        string userSid = string.Empty;
                        if (actionGetter.GetByte("UserRet", ref userRet) && userRet == (byte)1 &&
                            actionGetter.GetString("UserSid", ref userSid))
                        {
                            var switchSession = ServerSsMgr.GetSwitchSession();
                            //未连接上路由服，则发给大厅服，由大厅服转发
                            if (switchSession == null)
                            {
                                var paramStr = "ActionId=100&MsgId=0&UserSid=" + userSid;
                                string sign = SignUtils.EncodeSign(paramStr, RequestParam.SignKey);
                                paramStr += string.Format("&{0}={1}", "sign", sign);
                                var postData = Encoding.UTF8.GetBytes(string.Format("?d={0}", paramStr));
                                byte[] paramBytes = new byte[postData.Length + PackageReader.EnterChar.Length + data.Length];
                                Buffer.BlockCopy(postData, 0, paramBytes, 0, postData.Length);
                                Buffer.BlockCopy(PackageReader.EnterChar, 0, paramBytes, postData.Length, PackageReader.EnterChar.Length);
                                Buffer.BlockCopy(data, 0, paramBytes, postData.Length + PackageReader.EnterChar.Length, data.Length);

                                await session.SendAsync(actionGetter.OpCode, paramBytes, 0, paramBytes.Length, OnSendCompleted);
                            }
                            //已连接上路由服，则直接发给路由服
                            else
                            {
                                var paramStr = "ActionId=101&MsgId=0&UserSid=" + userSid;
                                string sign = SignUtils.EncodeSign(paramStr, RequestParam.SignKey);
                                paramStr += string.Format("&{0}={1}", "sign", sign);
                                var postData = Encoding.UTF8.GetBytes(string.Format("?d={0}", paramStr));
                                byte[] paramBytes = new byte[postData.Length + PackageReader.EnterChar.Length + data.Length];
                                Buffer.BlockCopy(postData, 0, paramBytes, 0, postData.Length);
                                Buffer.BlockCopy(PackageReader.EnterChar, 0, paramBytes, postData.Length, PackageReader.EnterChar.Length);
                                Buffer.BlockCopy(data, 0, paramBytes, postData.Length + PackageReader.EnterChar.Length, data.Length);

                                await switchSession.SendAsync(actionGetter.OpCode, paramBytes, 0, paramBytes.Length, OnSendCompleted);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TraceLog.WriteError("PostSend error:{0}", ex);
                }
            }
            catch (Exception ex)
            {
                TraceLog.WriteError("Task error:{0}", ex);
            }
            finally
            {
                if (session != null) session.ExitSession();
            }
        }

        /// <summary>
        /// Response hearbeat stream.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="session"></param>
        protected void ResponseHearbeat(RequestPackage package, GameSession session)
        {
            try
            {
                MessageStructure response = new MessageStructure();
                response.WriteBuffer(new MessageHead(package.MsgId, package.ActionId, 0));
                var data = response.PopBuffer();
                if (session != null && data.Length > 0)
                {
                    session.SendAsync(OpCode.Binary, data, 0, data.Length, OnSendCompleted).Wait();
                }
            }
            catch (Exception ex)
            {
                TraceLog.WriteError("Post Heartbeat error:{0}", ex);
            }
        }

        private void DoCheckHeartbeat(object state)
        {
            try
            {
                if (!Connected)
                {
                    return;
                }
                if (HeartPacket != null && HeartPacket.Length > 0)
                {
                    Send(HeartPacket);
                }
            }
            catch (Exception ex)
            {
                TraceLog.WriteError("Socket remote heartbeat error:{0}", ex);
            }
        }

        /// <summary>
        /// Send data success
        /// </summary>
        /// <param name="result"></param>
        protected virtual void OnSendCompleted(SocketAsyncResult result)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start(string[] args)
        {
            Connect();
            EntitySyncManger.SendHandle += (userId, data) =>
            {
                GameSession session = GameSession.Get(userId);
                if (session != null)
                {
                    var task = session.SendAsync(OpCode.Binary, data, 0, data.Length, result => { });
                    task.Wait();
                    return task.Result;
                }
                return false;
            };
            base.Start(args);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Stop()
        {
            OnServiceStopBefore();
            Close();
            OnServiceStop();
            try
            {
                //threadPool.Dispose();
                EntitySyncManger.Dispose();
                //threadPool = null;
            }
            catch
            {
            }
            base.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract void OnServiceStopBefore();


        /// <summary>
        /// Raises the service stop event.
        /// </summary>
        protected abstract void OnServiceStop();
    }
}