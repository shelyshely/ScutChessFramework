using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Contract.ServerCom;
using ZyGames.Framework.Game.Contract.SwitchServer;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.RPC.IO;

namespace Switch.Script.CsScript.Controller
{
    /// <summary>
    /// 连接服辅助
    /// </summary>
    class ConnectHelper
    {
        /// <summary>
        /// 连接服发送信息
        /// </summary>
        class ConnectSendInfo
        {
            public GameSession ConnectSs;
            public List<string> UserSidList;
        }

        /// <summary>
        /// 返回结果给连接服
        /// </summary>
        public static void SendResultToConnect(ActionGetter actionGetter, string[] userSidArray, out int errorCnt)
        {
            errorCnt = 0;
            try
            {
                var data = (byte[])actionGetter.GetMessage();
                var dict = new Dictionary<string/*serSid*/, ConnectSendInfo>();
                foreach (var item in userSidArray)
                {
                    var connectSs = SwitchSessionMgr.GetConnectSession(item);
                    if (connectSs == null)
                    {
                        TraceLog.WriteWarn("ConnectHelper SendResultToConnect error,item:{0}", item);
                        errorCnt++;
                        continue;
                    }
                    if (!dict.ContainsKey(connectSs.SessionId))
                    {
                        dict.Add(connectSs.SessionId, new ConnectSendInfo()
                            {
                                ConnectSs = connectSs,
                                UserSidList = new List<string>() { item }
                            });
                    }
                    else
                    {
                        dict[connectSs.SessionId].UserSidList.Add(item);
                    }
                }
                //
                SendResultToConnect(dict, data);
            }
            catch
            {
                TraceLog.WriteError("ConnectHelper SendResultToConnect error,catch");
            }
        }

        /// <summary>
        /// 返回结果给连接服
        /// </summary>
        private static async void SendResultToConnect(Dictionary<string/*serSid*/, ConnectSendInfo> dict, byte[] data)
        {
            foreach (var item in dict)
            {
                var paramStr = "ActionId=102&MsgId=0&UserSid=" + string.Join(";", item.Value.UserSidList);
                string sign = SignUtils.EncodeSign(paramStr, RequestParam.SignKey);
                paramStr += string.Format("&{0}={1}", "sign", sign);
                var postData = Encoding.UTF8.GetBytes(string.Format("?d={0}", paramStr));
                byte[] paramBytes = new byte[postData.Length + PackageReader.EnterChar.Length + data.Length];
                Buffer.BlockCopy(postData, 0, paramBytes, 0, postData.Length);
                Buffer.BlockCopy(PackageReader.EnterChar, 0, paramBytes, postData.Length, PackageReader.EnterChar.Length);
                Buffer.BlockCopy(data, 0, paramBytes, postData.Length + PackageReader.EnterChar.Length, data.Length);

                await item.Value.ConnectSs.SendAsync(paramBytes, 0, paramBytes.Length);
            }
        }

        /// <summary>
        /// 全服广播结果
        /// </summary>
        public static void BroadcastAll(ActionGetter actionGetter, out int errorCnt)
        {
            errorCnt = 0;
            try
            {
                var data = (byte[])actionGetter.GetMessage();
                var dict = new Dictionary<string/*serSid*/, ConnectSendInfo>();
                foreach (var item in SwitchSessionMgr.GetOnlineAll())
                {
                    var connectSs = SwitchSessionMgr.GetConnectSession(item);
                    if (connectSs == null)
                    {
                        TraceLog.WriteWarn("ConnectHelper BroadcastAll error,item:{0}", item);
                        errorCnt++;
                        continue;
                    }
                    if (!dict.ContainsKey(connectSs.SessionId))
                    {
                        dict.Add(connectSs.SessionId, new ConnectSendInfo()
                        {
                            ConnectSs = connectSs,
                            UserSidList = new List<string>() { item }
                        });
                    }
                    else
                    {
                        dict[connectSs.SessionId].UserSidList.Add(item);
                    }
                }
                //
                SendResultToConnect(dict, data);
            }
            catch
            {
                TraceLog.WriteError("ConnectHelper BroadcastAll error,catch");
            }
        }
    }
}
