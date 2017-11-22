using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Game.Contract.Action;
using ZyGames.Framework.Game.Service;

namespace Switch.Script.CsScript.Action
{
    public abstract class BaseAction : AuthorizeAction
    {
        protected BaseAction(short actionId, ActionGetter httpGet)
            : base(actionId, httpGet)
        {
        }
    }
}
