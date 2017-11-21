using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Common;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Runtime;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.Net;
using ZyGames.Framework.RPC.IO;
using ZyGames.Framework.RPC.Sockets;

namespace ZyGames.Framework.Game.Contract.ServerCom
{
    /// <summary>
    /// 游戏服专用推送
    /// (除连接服、路由服、大厅服之外的负责具体游戏的服务器)
    /// </summary>
    public class GSBroadCastActionFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="sessionList"></param>
        /// <param name="package"></param>
        /// <param name="complateHandle"></param>
        /// <param name="onlineInterval"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task BroadcastAction(int actionId, GameSession session, object parameter, Action<GameSession, SocketAsyncResult> complateHandle, int onlineInterval)
        {
            try
            {
                sbyte opCode = OpCode.Binary;
                RequestPackage package = parameter is Parameters
                    ? ActionFactory.GetResponsePackage(actionId, session, parameter as Parameters, opCode, null)
                    : ActionFactory.GetResponsePackage(actionId, session, null, opCode, parameter);
                if (session.Equals(null))
                {
                    throw new ArgumentNullException("Session is a null value.");
                }

                IActionDispatcher actionDispatcher = new ScutActionDispatcher();
                package.Bind(session);
                var actionGetter = new HttpGet(package, session);
                byte[] data = ProcessActionResponse(actionDispatcher, actionId, actionGetter);
                GameSession temp = session;
                try
                {
                    if (onlineInterval <= 0 || session.LastActivityTime > MathUtils.Now.AddSeconds(-onlineInterval))
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

                                await session.SendAsync(package.OpCode, paramBytes, 0, paramBytes.Length, result =>
                                {
                                    if (complateHandle != null)
                                    {
                                        complateHandle(temp, result);
                                    }
                                });
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

                                await switchSession.SendAsync(package.OpCode, paramBytes, 0, paramBytes.Length, result =>
                                {
                                    if (complateHandle != null)
                                    {
                                        complateHandle(temp, result);
                                    }
                                });
                            }
                        }
                        else
                        {
                            await session.SendAsync(package.OpCode, data, 0, data.Length, result =>
                            {
                                if (complateHandle != null)
                                {
                                    complateHandle(temp, result);
                                }
                            });
                        } 
                    }
                    else
                    {
                        if (complateHandle != null)
                        {
                            complateHandle(temp, new SocketAsyncResult(data) { Result = ResultCode.Close });
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (complateHandle != null)
                    {
                        complateHandle(temp, new SocketAsyncResult(data) { Result = ResultCode.Error, Error = ex });
                    }
                    TraceLog.WriteError("SocialBroadCastActionFactory BroadcastAction  action:{0} userId:{1} error:{2}", actionId, session.UserId, ex);
                }
            }
            catch (Exception ex)
            {
                TraceLog.WriteError("SocialBroadCastActionFactory BroadcastAction  action:{0} error:{1}", actionId, ex);
            }
        }

        /// <summary>
        /// 获取Action处理的输出字节流
        /// </summary>
        /// <returns></returns>
        private static byte[] ProcessActionResponse(IActionDispatcher actionDispatcher, int actionId, ActionGetter actionGetter)
        {
            BaseStruct baseStruct = ActionFactory.FindRoute(GameEnvironment.Setting.ActionTypeName, actionGetter, actionId);
            SocketGameResponse response = new SocketGameResponse();
            response.WriteErrorCallback += actionDispatcher.ResponseError;
            baseStruct.SetPush();
            baseStruct.DoInit();
            if (actionGetter.Session.EnterLock(actionId))
            {
                try
                {
                    if (!baseStruct.GetError() &&
                        baseStruct.ReadUrlElement() &&
                        baseStruct.DoAction() &&
                        !baseStruct.GetError())
                    {
                        baseStruct.WriteResponse(response);
                    }
                    else
                    {
                        baseStruct.WriteErrorAction(response);
                    }
                }
                finally
                {
                    actionGetter.Session.ExitLock(actionId);
                }
            }
            else
            {
                baseStruct.WriteLockTimeoutAction(response, false);
            }
            return response.ReadByte();
        }
    }
}
