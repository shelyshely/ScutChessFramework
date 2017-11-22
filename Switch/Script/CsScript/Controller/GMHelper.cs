using Switch.Script.CsScript.Action;
using Switch.Script.CsScript.Defines;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Common;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Game.Contract;
using ZyGames.Framework.RPC.IO;
using System.Web;
using ZyGames.Framework.Common.Log;
using ZyGames.Framework.Game.Contract.SwitchServer;

namespace Switch.Script.CsScript.Controller
{
    /// <summary>
    /// GM辅助器
    /// </summary>
    public class GMHelper
    {
        /// <summary>
        /// 口令key
        /// </summary>
        public const string _wordkey = "5FA00358DF33411DA5E89E0B1832D495";

        /// <summary>
        /// 检查GM口令
        /// </summary>
        /// <param name="word">口令</param>
        public static bool checkWordCorrect(string word, string wordKey = _wordkey)
        {
            var strArray = word.Split(';');
            if (strArray.Length == 2)
            {
                string key = ZyGames.Framework.Common.Security.CryptoHelper.MD5_Encrypt(strArray[0] + wordKey, Encoding.UTF8);
                if (!string.IsNullOrEmpty(key) && key.ToLower() == strArray[1])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
