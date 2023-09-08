using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    public partial class BlankIconInfo
    {
        public BlankIconInfo() { }

        #region Model
        private int _id;
        private string _type;
        private string _disboxcode;
        private double _rtndirection;
        private int _escapelineid;

        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 对象所在的表名
        /// </summary>
        public string Type
        {
            set { _type = value; }
            get { return _type; }
        }

        /// <summary>
        /// 空白灯具所在的EPS码
        /// </summary>
        public string DisboxCode
        {
            set { _disboxcode = value; }
            get { return _disboxcode; }
        }

        /// <summary>
        /// 空白灯具图标旋转的角度
        /// </summary>
        public double RtnDirection
        {
            set { _rtndirection = value; }
            get { return _rtndirection; }
        }

        /// <summary>
        /// 空白灯具绑定的逃生路线序号
        /// </summary>
        public int EscapeLineID
        {
            set { _escapelineid = value; }
            get { return _escapelineid; }
        }
        #endregion
    }
}
