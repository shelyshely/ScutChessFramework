using Switch.Script.CsScript.Controller;
using System;
using System.Collections.Generic;
using System.Text;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Service;

namespace Switch.Script.CsScript.Action
{

    /// <summary>
    /// 接收其它服结果广播给所有玩家
    /// </summary>
    /// <remarks>继续BaseStruct类:允许无身份认证的请求;AuthorizeAction:需要身份认证的请求</remarks>
    public class Action105 : BaseAction
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


        public Action105(ActionGetter actionGetter)
            : base(ActionIDDefine.CstAction105, actionGetter)
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
            return true;
        }

        /// <summary>
        /// 业务逻辑处理
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool TakeAction()
        {
            int errorCnt; 
            ConnectHelper.BroadcastAll(actionGetter, out errorCnt);
            if (errorCnt > 0)
            {
                TraceLog.WriteWarn("Action105 error,errorCnt:{0}", errorCnt);
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
