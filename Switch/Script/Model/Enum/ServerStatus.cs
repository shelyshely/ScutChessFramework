using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Switch.Script.Model.Enum
{
    /// <summary>
    /// 服务器状态
    /// </summary>
    public enum ServerStatus
    {
        /// <summary>
        /// 未连接
        /// </summary>
        NotConnected = 0,
        /// <summary>
        /// 已连接
        /// </summary>
        Connected = 1,
        /// <summary>
        /// 关闭中
        /// </summary>
        Closeing = 2,
        /// <summary>
        /// 已关闭
        /// </summary>
        Closed = 3,
    }
}
