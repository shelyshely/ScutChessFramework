using Switch.Script.CsScript.Controller;
using Switch.Script.CsScript.Defines;
using System;
using System.Collections.Generic;
using System.Text;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Common.Security;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.RPC.IO;


namespace Switch.Script.CsScript.Action
{

    /// <summary>
    /// 房间服登录路由服
    /// </summary>
    /// <remarks>继续BaseStruct类:允许无身份认证的请求;AuthorizeAction:需要身份认证的请求</remarks>
    public class Action16 : BaseAction
    {

        #region class object
        /// <summary>
        /// Main Body
        /// </summary>
        class ResponsePacket
        {
            /// <summary>
            /// 1成功
            /// </summary>
            public byte Ret { get; set; }

        }
        #endregion

        /// <summary>
        /// 响应数据包
        /// </summary>
        private ResponsePacket _packet = new ResponsePacket();
        /// <summary>
        /// 
        /// </summary>
        private int _serverId;
        /// <summary>
        /// 时间戳
        /// </summary>
        private long _tstamp;
        /// <summary>
        /// 口令
        /// </summary>
        private string _word;


        public Action16(ActionGetter actionGetter)
            : base(ActionIDDefine.CstAction16, actionGetter)
        {

        }

        /// <summary>
        /// 检查的Action是否需要授权访问
        /// </summary>
        protected override bool IgnoreActionId
        {
            get { return true; }
        }

        /// <summary>
        /// 客户端请求的参数较验
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool GetUrlElement()
        {
            httpGet.GetInt("ServerId", ref _serverId);
            httpGet.GetLong("Tstamp", ref _tstamp);
            httpGet.GetString("Word", ref _word);

            return true;
        }

        /// <summary>
        /// 业务逻辑处理
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool TakeAction()
        {
            string key = CryptoHelper.MD5_Encrypt("" + _serverId + _tstamp + GlobalDefine.LoginSwitch_Key, Encoding.UTF8);
            if (key == _word && !ServerHelper.IsExistRS(_serverId))
            {
                var freeUserId = ServerHelper.GetFreeRSUserId();
                if(freeUserId > 0)
                {
                    Current.Bind(new SessionUser() { UserId = freeUserId, PassportId = string.Format("RoomS-{0}", _serverId) });
                    //设置忽略锁
                    Current.SetIgoreLock(true, actionId);
                    ServerHelper.AddRS(_serverId, Current);
                    _packet.Ret = 1;
                }
                else
                {
                    TraceLog.WriteError("所有给其它服分配的UserId已用完！");
                }
            }
            else
            {
                _packet.Ret = 0;
            }

            //
            SendLoginResult();
            //
            IsNotRespond = true;
            
            return true;
        }

        public async void SendLoginResult()
        {
            RequestParam param = new RequestParam();
            param["ActionId"] = ActionIDDefine.CstAction17;
            param["msgid"] = 0;
            param["Ret"] = _packet.Ret;
            string post = string.Format("?d={0}", System.Web.HttpUtility.UrlEncode(param.ToPostString()));
            var data = Encoding.UTF8.GetBytes(post);
            await Current.SendAsync(actionGetter.OpCode, data, 0, data.Length, null);
        }

        /// <summary>
        /// 下发给客户的包结构数据
        /// </summary>
        public override void BuildPacket()
        {
        }

    }
}
