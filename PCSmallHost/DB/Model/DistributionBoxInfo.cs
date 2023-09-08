using System;
namespace PCSmallHost.DB.Model
{
    /// <summary>
    /// EPS实体类
    /// </summary>
    [Serializable]
    public partial class DistributionBoxInfo : IComparable<DistributionBoxInfo>
    {
        public DistributionBoxInfo()
        { }
        #region Model
        private int _id;
        private string _code;
        private string _address;
        private int _status;
        private double _mainelevoltage;
        private double _mainelecurrent;
        private double _batteryvoltage;
        private double _dischargecurrent;
        private string _errortime;
        private int _disable;
        private int _plan1;
        private int _plan2;
        private int _plan3;
        private int _plan4;
        private int _plan5;
        private int _isEmergency;
        private int _automanual;
        private int _test;
        private int _qiangqi;
        private int? _shield;

        /// <summary>
        /// 
        /// </summary>
        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 配电箱码
        /// </summary>
        public string Code
        {
            set { _code = value; }
            get { return _code; }
        }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status
        {
            set { _status = value; }
            get { return _status; }
        }

        /// <summary>
        /// 主电电压
        /// </summary>
        public double MainEleVoltage
        {
            get { return _mainelevoltage; }
            set { _mainelevoltage = value; }
        }

        /// <summary>
        /// 输出电压
        /// </summary>
        public double DischargeVoltage
        {
            get { return _mainelecurrent; }
            set { _mainelecurrent = value; }
        }

        /// <summary>
        /// 电池电压
        /// </summary>
        public double BatteryVoltage
        {
            get { return _batteryvoltage; }
            set { _batteryvoltage = value; }
        }

        /// <summary>
        /// 放电电流
        /// </summary>
        public double DischargeCurrent
        {
            get { return _dischargecurrent; }
            set { _dischargecurrent = value; }
        }

        /// <summary>
        /// 错误记录时间
        /// </summary>
        public string ErrorTime
        {
            set { _errortime = value; }
            get { return _errortime; }
        }

        /// <summary>
        /// 是否故障屏蔽
        /// </summary>
        public int Disable
        {
            set { _disable = value; }
            get { return _disable; }
        }

        /// <summary>
        /// 预案1
        /// </summary>
        public int Plan1
        {
            get { return _plan1; }
            set { _plan1 = value; }
        }

        /// <summary>
        /// 预案2
        /// </summary>
        public int Plan2
        {
            get { return _plan2; }
            set { _plan2 = value; }
        }

        /// <summary>
        /// 预案3
        /// </summary>
        public int Plan3
        {
            get { return _plan3; }
            set { _plan3 = value; }
        }

        /// <summary>
        /// 预案4
        /// </summary>
        public int Plan4
        {
            get { return _plan4; }
            set { _plan4 = value; }
        }

        /// <summary>
        /// 预案5
        /// </summary>
        public int Plan5
        {
            get { return _plan5; }
            set { _plan5 = value; }
        }

        /// <summary>
        /// 是否应急
        /// </summary>
        public int IsEmergency
        {
            set { _isEmergency = value; }
            get { return _isEmergency; }
        }

        /// <summary>
        /// EPS自动或手动，自动为0，手动为1,两者都不是则为2
        /// </summary>
        public int AutoManual
        {
            set { _automanual = value; }
            get { return _automanual; }
        }

        /// <summary>
        /// EPS测试状态
        /// </summary>
        public int Test
        {
            set { _test = value; }
            get { return _test; }
        }

        /// <summary>
        /// EPS强启
        /// </summary>
        public int QiangQi
        {
            set { _qiangqi = value; }
            get { return _qiangqi; }
        }

        /// <summary>
        /// 是否屏蔽
        /// </summary>
        public int? Shield
        {
            set { _shield = value; }
            get { return _shield; }
        }

        public int CompareTo(DistributionBoxInfo other)
        {
            return _code.CompareTo(other._code);
        }
        #endregion Model
    }
}

