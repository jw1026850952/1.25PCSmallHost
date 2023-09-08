using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    /// <summary>
    /// 火警联动接收信息实体类
    /// </summary>
    public class FireAlarmZoneInfo
    {        
        private int _firealarmlinkzonenumber;
        private int _firealarmlinkstatcount;        
        private bool _isfirealarmlinknow;

        /// <summary>
        /// 是否为当前发生火灾分区
        /// </summary>
        public bool IsFireAlarmLinkNow
        {
            get { return _isfirealarmlinknow; }
            set { _isfirealarmlinknow = value; }
        }

        /// <summary>
        /// 火灾分区统计数量
        /// </summary>
        public int FireAlarmLinkStatCount
        {
            get { return _firealarmlinkstatcount; }
            set { _firealarmlinkstatcount = value; }
        }

        /// <summary>
        /// 分区区号
        /// </summary>
        public int FireAlarmLinkZoneNumber
        {
            get { return _firealarmlinkzonenumber; }
            set { _firealarmlinkzonenumber = value; }
        }
    }
}
