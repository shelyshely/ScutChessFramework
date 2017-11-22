using Switch.Script.CsScript.Controller;
using System;
using System.Collections.Generic;
using System.Text;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Service;

namespace Switch.Script.CsScript.Action
{

    /// <summary>
    /// 接收其它服结果返回给玩家
    /// </summary>
    /// <remarks>继续BaseStruct类:允许无身份认证的请求;AuthorizeAction:需要身份认证的请求</remarks>
    public class Action101 : BaseAction
    {

        #region class object
        /// <summary>
        /// Main Body
        /// </summary>
        class ResponsePacket
        {

        }
        #endregion

        /// <summary>
        /// 响应数据包
        /// </summary>
        private ResponsePacket _packet = new ResponsePacket();
        /// <summary>
        /// 
        /// </summary>
        private string _userSid;


        public Action101(ActionGetter actionGetter)
            : base(ActionIDDefine.CstAction101, actionGetter)
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
            if (httpGet.GetString("UserSid", ref _userSid))
            {
                return true;
            }
            TraceLog.WriteError("Action101 not have 'UserSid' 参数！");
            return false;
        }

        /// <summary>
        /// 业务逻辑处理
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool TakeAction()
        {
            int errorCnt; 
            ConnectHelper.SendResultToConnect(actionGetter, _userSid.Split(';'), out errorCnt);
            if (errorCnt > 0)
            {
                TraceLog.WriteWarn("Action101 error,UserSid:{0},errorCnt:{1}", _userSid, errorCnt);
            }
            //
            IsNotRespond = true;
            return true;
        }

        /// <summary>
        /// 下发给客户的包结构数据
        /// </summary>
        public override void BuildPacket()
        {

        }

    }
}
