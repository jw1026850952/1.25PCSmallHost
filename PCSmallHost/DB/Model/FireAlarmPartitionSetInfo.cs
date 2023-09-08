using System;
namespace PCSmallHost.DB.Model
{
    /// <summary>
    /// 分区设置实体类
    /// </summary>
    [Serializable]
    public partial class FireAlarmPartitionSetInfo : IComparable<FireAlarmPartitionSetInfo>
    {
        public FireAlarmPartitionSetInfo()
        { }
        #region Model
        private int _id;
        private int _planpartition;
        private int _floor;
        private int _mainboardcircuit;
        private string _deviceclass;
        private int _lowdevicerange;
        private int _highdevicerange;

        /// <summary>
        /// 
        /// </summary>
        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 预案分区
        /// </summary>
        public int PlanPartition
        {
            set { _planpartition = value; }
            get { return _planpartition; }
        }

        /// <summary>
        /// 层
        /// </summary>
        public int Floor
        {
            set { _floor = value; }
            get { return _floor; }
        }

        /// <summary>
        /// 主板回路
        /// </summary>
        public int MainBoardCircuit
        {
            set { _mainboardcircuit = value; }
            get { return _mainboardcircuit; }
        }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string DeviceClass
        {
            set { _deviceclass = value; }
            get { return _deviceclass; }
        }

        /// <summary>
        /// 设备范围低
        /// </summary>
        public int LowDeviceRange
        {
            set { _lowdevicerange = value; }
            get { return _lowdevicerange; }
        }

        /// <summary>
        /// 设备范围高
        /// </summary>
        public int HighDeviceRange
        {
            set { _highdevicerange = value; }
            get { return _highdevicerange; }
        }
        #endregion Model
        
        public int CompareTo(FireAlarmPartitionSetInfo other)
        {
            return _planpartition.CompareTo(other._planpartition);
        }
    }
}

