using Switch.Script.CsScript.Controller;
using System;
using System.Collections.Generic;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.Game.Lang;
using ZyGames.Framework.Game.Service;


namespace Switch.Script.CsScript.Action
{

    /// <summary>
    /// 动态编译定时器重启
    /// </summary>
    /// <remarks>继续BaseStruct类:允许无身份认证的请求;AuthorizeAction:需要身份认证的请求</remarks>
    public class Action90016 : BaseStruct
    {

        #region class object
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private string _word;

        private string _actionType;


        public Action90016(ActionGetter actionGetter)
            : base(ActionIDDefine.CstAction90016, actionGetter)
        {

        }

        /// <summary>
        /// 客户端请求的参数较验
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool GetUrlElement()
        {
            IsNotRespond = true;
            //
            if (httpGet.GetString("Word", ref _word)
                && httpGet.GetString("ActionType", ref _actionType))
            {
                if (!GMHelper.checkWordCorrect(_word))
                {
                    ErrorCode = Language.Instance.ErrorCode;
                    ErrorInfo = "非法操作";
                    TraceLog.WriteError("Action90016非法操作");
                    return false;
                }
                //执行
                switch (_actionType)
                {
                    case "OnStartAffer":
                        {
                            ServerHelper.Init();
                            TimerHelper.StartTimer();
                        } break;
                    case "OnSessionRecover":        //玩家session恢复
                        {
                            //大厅服写了就可以，此次不需要
                        } break;
                    case "OnScriptLoaded":
                        {
                            TraceLog.WriteInfo("动态编译后重启定时器...");
                            TimerHelper.StartTimer();
                            TraceLog.WriteInfo("动态编译后重启定时器完成");
                        } break;
                    case "OnClientDisconnected":    //断开
                        {
                            var message = httpGet.GetMessage();
                            if (message is GameSession)
                            {
                                var session = (GameSession)message;
                                if (session != null)
                                {
                                    ServerHelper.OnOtherServerDisconnected(session);
                                }
                            }
                        }break;
                    default:
                        break;
                }
                return true;
            }
            ErrorCode = Language.Instance.ErrorCode;
            ErrorInfo = Language.Instance.UrlElement;
            TraceLog.WriteError("Action90016获取参数失败");
            return false;
        }

        /// <summary>
        /// 业务逻辑处理
        /// </summary>
        /// <returns>false:中断后面的方式执行并返回Error</returns>
        public override bool TakeAction()
        {
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
