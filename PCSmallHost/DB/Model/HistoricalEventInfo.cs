using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    public class HistoricalEventInfo
    {
        public HistoricalEventInfo()
        { }
        #region Model
        private int _id;
        private string _eventtime;
        private string _eventcontent;

        /// <summary>
        /// 
        /// </summary>
        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 事件发生时间
        /// </summary>
        public string EventTime
        {
            set { _eventtime = value; }
            get { return _eventtime; }
        }

        /// <summary>
        /// 事件内容
        /// </summary>
        public string EventContent
        {
            set { _eventcontent = value; }
            get { return _eventcontent; }
        }
        #endregion
    }
}
