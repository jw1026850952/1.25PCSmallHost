using System;
namespace PCSmallHost.DB.Model
{
    /// <summary>
    /// 火灾报警器实体类
    /// </summary>
    [Serializable]
    public partial class FireAlarmTypeInfo
    {
        public FireAlarmTypeInfo()
        { }
        #region Model
        private int _id;
        private string _firealarmcode;
        private string _firealarmname;      
        private int _iscurrentfirealarm;
       
        /// <summary>
        /// 
        /// </summary>
        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 火灾报警器类型所对应的代码文件名称
        /// </summary>
        public string FireAlarmCode
        {
            set { _firealarmcode = value; }
            get { return _firealarmcode; }
        }

        /// <summary>
        /// 火灾报警器类型名称
        /// </summary>
        public string FireAlarmName
        {
            set { _firealarmname = value; }
            get { return _firealarmname; }
        }

        /// <summary>
        /// 是否为当前选中的火灾报警器
        /// </summary>
        public int IsCurrentFireAlarm
        {
            set { _iscurrentfirealarm = value; }
            get { return _iscurrentfirealarm; }
        }
        #endregion Model

    }
}

