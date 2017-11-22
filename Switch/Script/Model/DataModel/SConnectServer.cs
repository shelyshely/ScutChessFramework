using Switch.Script.Model.Enum;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZyGames.Framework.Cache.Generic;
using ZyGames.Framework.Game.Context;
using ZyGames.Framework.Game.Service;
using ZyGames.Framework.Model;

namespace Switch.Script.Model.DataModel
{
    /// <summary>
    /// 连接服信息共享实体
    /// (只存redis不存db)
    /// </summary>
    [Serializable, ProtoContract]
    [EntityTable(AccessLevel.ReadWrite, CacheType.Entity, false)]
    public class SConnectServer : ShareEntity
    {
        /// <summary>
        /// </summary>
        public SConnectServer()
            : base(false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(1)]
        [EntityField(true)]
        public int ServerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(2)]
        [EntityField()]
        public ServerStatus Status { get; set; }

        /// <summary>
        /// 连接服的sessionId
        /// </summary>
        [ProtoMember(3)]
        [EntityField()]
        public string CSSid { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [ProtoMember(4)]
        [EntityField()]
        public int UserId { get; set; }

        /// <summary>
        /// 在线人数
        /// </summary>
        [ProtoMember(5)]
        [EntityField()]
        public int OnlineCnt { get; set; }

        protected override int GetIdentityId()
        {
            return ServerId;
        }

    }
}
