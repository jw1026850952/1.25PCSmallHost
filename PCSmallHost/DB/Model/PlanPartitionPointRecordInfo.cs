using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    public class PlanPartitionPointRecordInfo
    {
        public PlanPartitionPointRecordInfo()
        { }
        #region Model
        private int _id;
        private int _planpartition;
        private int? _escapelineid;

        /// <summary>
        /// 
        /// </summary>
        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 预案分区/报警号
        /// </summary>
        public int PlanPartition
        {
            set { _planpartition = value; }
            get { return _planpartition; }
        }

        public int? EscapeLineID
        {
            set { _escapelineid = value; }
            get { return _escapelineid; }
        }
        #endregion Model
    }
}
