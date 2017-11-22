using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Common.Configuration;

namespace Switch.Script.CsScript.Defines
{
    class GlobalDefine
    {
        /// <summary>
        /// 服Id类型
        /// </summary>
        public enum ServerIdType
        {
            //大厅服专用UserId
            LobbyId = 2016,
            //路由服专用UserId
            SwitchId,
            //社交服专用UserId
            SocialId,
            //拉霸机服专用UserId
            SuperMonreyId,
            //挖豹子服专用UserId
            WabaoziId,
        }
        //给房间服分配的UserId
        public const int RS_UserId_Min = 300;
        public const int RS_UserId_Max = 350;
        //给连接服分配的UserId
        public const int CS_UserId_Min = 400;
        public const int CS_UserId_Max = 450;

        //登录路由服的Key
        public const string LoginSwitch_Key = "D4338529-A306-4846-B944-234FF8F08175";
        //是否调试状态
        public static readonly bool ScriptIsDebug = ConfigUtils.GetSetting("Script_IsDebug", false);
    }
}
