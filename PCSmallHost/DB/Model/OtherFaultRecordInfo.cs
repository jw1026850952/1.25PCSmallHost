using System;
namespace PCSmallHost.DB.Model
{
    /// <summary>
    /// OtherFaultRecordInfo:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    public partial class OtherFaultRecordInfo
    {
        public OtherFaultRecordInfo()
        { 

        }
        #region Model
        private int _id;
        private string _description;
        private int _isexist;
        private int _disable;
        /// <summary>
        /// 
        /// </summary>
        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 故障描述
        /// </summary>
        public string Description
        {
            set { _description = value; }
            get { return _description; }
        }

        /// <summary>
        /// 是否存在故障
        /// </summary>
        public int IsExist
        {
            set { _isexist = value; }
            get { return _isexist; }
        }

        /// <summary>
        /// 是否屏蔽故障
        /// </summary>
        public int Disable
        {
            set { _disable = value; }
            get { return _disable; }
        }
        #endregion Model

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            OtherFaultRecordInfo other = (OtherFaultRecordInfo)obj;

            return _id == other._id &&
                   _description == other._description &&
                   _isexist == other._isexist &&
                   _disable == other._disable;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + _id.GetHashCode();
                hash = hash * 23 + (_description != null ? _description.GetHashCode() : 0);
                hash = hash * 23 + _isexist.GetHashCode();
                hash = hash * 23 + _disable.GetHashCode();
                return hash;
            }
        }

    }
}

