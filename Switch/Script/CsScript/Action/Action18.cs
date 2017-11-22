using Switch.Script.CsScript.Controller;
using System;
using System.Collections.Generic;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Contract.ServerCom;
using ZyGames.Framework.Game.Service;


namespace Switch.Script.CsScript.Action
{

    /// <summary>
    /// 通知路由服连接服准备关闭
    /// </summary>
    /// <remarks>继续BaseStruct类:允许无身份认证的请求;AuthorizeAction:需要身份认证的请求</remarks>
    public class Action18 : BaseAction
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


        public Action18(ActionGetter actionGetter)
            : base((short)18, actionGetter)
        {

        }

        /// <summary>
        /// 检查的Action是否需要授权访问
        /// </summary>
        protected override bool IgnoreActionId
        {
            get { return false; }
        }

        /// <summary>
        /// 客户端请求的参数较验
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool GetUrlElement()
        {
            return true;
        }

        /// <summary>
        /// 业务逻辑处理
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool TakeAction()
        {
            if (ServerSsMgr.Get(Current.SessionId) != null)
            {
                ServerHelper.OnCloseCSBefore(Current);
            }
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
