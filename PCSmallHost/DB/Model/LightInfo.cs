using System;
namespace PCSmallHost.DB.Model
{
    /// <summary>
    /// 灯具实体类
    /// </summary>
    [Serializable]
    public partial class LightInfo : IComparable<LightInfo>
    {
        public LightInfo()
        { }
        #region Model
        private int _id;
        private string _code;
        private string _address;
        private int _status;
        private int _beginstatus;
        private int _currentstate;
        private string _errortime;
        private int _disable;
        private int _planleft1;
        private int _planleft2;
        private int _planleft3;
        private int _planleft4;
        private int _planleft5;
        private int _planright1;
        private int _planright2;
        private int _planright3;
        private int _planright4;
        private int _planright5;
        private int _lightclass;
        private int _disboxid;
        private int _lightindex;
        private int _isEmergency;
        private double _rtndirection;
        private int? _escapelineid;
        private int? _shield;

        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 灯码
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
        /// 当前状态位
        /// </summary>
        public int Status
        {
            set { _status = value; }
            get { return _status; }
        }

        /// <summary>
        /// 灯具当前状态(亮、灭、闪、主电、左亮、右亮)
        /// </summary>
        public int CurrentState
        {
            set { _currentstate = value; }
            get { return _currentstate; }
        }

        /// <summary>
        /// 初始状态
        /// </summary>
        public int BeginStatus
        {
            set { _beginstatus = value; }
            get { return _beginstatus; }
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
        /// 左预案1
        /// </summary>
        public int PlanLeft1
        {
            set { _planleft1 = value; }
            get { return _planleft1; }
        }

        /// <summary>
        /// 左预案2
        /// </summary>
        public int PlanLeft2
        {
            set { _planleft2 = value; }
            get { return _planleft2; }
        }

        /// <summary>
        /// 左预案3
        /// </summary>
        public int PlanLeft3
        {
            set { _planleft3 = value; }
            get { return _planleft3; }
        }

        /// <summary>
        /// 左预案4
        /// </summary>
        public int PlanLeft4
        {
            set { _planleft4 = value; }
            get { return _planleft4; }
        }

        /// <summary>
        /// 左预案5
        /// </summary>
        public int PlanLeft5
        {
            set { _planleft5 = value; }
            get { return _planleft5; }
        }

        /// <summary>
        /// 右预案1
        /// </summary>
        public int PlanRight1
        {
            set { _planright1 = value; }
            get { return _planright1; }
        }

        /// <summary>
        /// 右预案2
        /// </summary>
        public int PlanRight2
        {
            set { _planright2 = value; }
            get { return _planright2; }
        }

        /// <summary>
        /// 右预案3
        /// </summary>
        public int PlanRight3
        {
            set { _planright3 = value; }
            get { return _planright3; }
        }

        /// <summary>
        /// 右预案4
        /// </summary>
        public int PlanRight4
        {
            set { _planright4 = value; }
            get { return _planright4; }
        }

        /// <summary>
        /// 右预案5
        /// </summary>
        public int PlanRight5
        {
            set { _planright5 = value; }
            get { return _planright5; }
        }

        /// <summary>
        /// 灯具类型
        /// </summary>
        public int LightClass
        {
            set { _lightclass = value; }
            get { return _lightclass; }
        }

        /// <summary>
        /// 配电箱ID
        /// </summary>
        public int DisBoxID
        {
            set { _disboxid = value; }
            get { return _disboxid; }
        }

        /// <summary>
        /// 灯具在配电箱下的索引
        /// </summary>
        public int LightIndex
        {
            set { _lightindex = value; }
            get { return _lightindex; }
        }

        /// <summary>
        /// 是否应急
        /// </summary>
        public int IsEmergency
        {
            get { return _isEmergency; }
            set { _isEmergency = value; }
        }
        /// <summary>
        /// 图标在图层上旋转的角度
        /// </summary>
        public double RtnDirection
        {
            set { _rtndirection = value; }
            get { return _rtndirection; }
        }
        /// <summary>
        /// 灯具所依赖的路线ID
        /// </summary>
        public int? EscapeLineID
        {
            set { _escapelineid = value; }
            get { return _escapelineid; }
        }

        public int? Shield
        {
            set { _shield = value; }
            get { return _shield; }
        }

        public int CompareTo(LightInfo other)
        {
            return _code.CompareTo(other._code);
        }
        #endregion Model

    }
}

