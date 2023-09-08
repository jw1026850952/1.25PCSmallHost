using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    public class EscapeLinesInfo
    {

        public EscapeLinesInfo()
        { }

        #region Model

        private int _id;
        private string _name;
        private int _location;
        private double _lineX1;
        private double _lineY1;
        private double _lineX2;
        private double _lineY2;
        private double _transformx1;
        private double _transformy1;
        private double _transformx2;
        private double _transformy2;
        private double _nlinex1;
        private double _nliney1;
        private double _nlinex2;
        private double _nliney2;

        /// <summary>
        /// 
        /// </summary>
        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 线段名字
        /// </summary>
        public string Name
        {
            set { _name = value; }
            get { return _name; }
        }

        /// <summary>
        /// 所在位置
        /// </summary>
        public int Location
        {
            set { _location = value; }
            get { return _location; }
        }

        /// <summary>
        /// 线段端点1的横坐标
        /// </summary>
        public double LineX1
        {
            set { _lineX1 = value; }
            get { return _lineX1; }
        }

        /// <summary>
        /// 线段端点1的纵坐标
        /// </summary>
        public double LineY1
        {
            set { _lineY1 = value; }
            get { return _lineY1; }
        }

        /// <summary>
        /// 线段端点2的横坐标
        /// </summary>
        public double LineX2
        {
            set { _lineX2 = value; }
            get { return _lineX2; }
        }

        /// <summary>
        /// 线段端点2的纵坐标
        /// </summary>
        public double LineY2
        {
            set { _lineY2 = value; }
            get { return _lineY2; }
        }

        /// <summary>
        /// 转换后线段端点1的横坐标
        /// </summary>
        public double TransformX1
        {
            set { _transformx1 = value; }
            get { return _transformx1; }
        }

        /// <summary>
        /// 转换后线段端点1的纵坐标
        /// </summary>
        public double TransformY1
        {
            set { _transformy1 = value; }
            get { return _transformy1; }
        }

        /// <summary>
        /// 转换后线段端点2的横坐标
        /// </summary>
        public double TransformX2
        {
            set { _transformx2 = value; }
            get { return _transformx2; }
        }

        /// <summary>
        /// 转换后线段端点2的纵坐标
        /// </summary>
        public double TransformY2
        {
            set { _transformy2 = value; }
            get { return _transformy2; }
        }

        /// <summary>
        /// 未登录图层线段端点1的横坐标
        /// </summary>
        public double NLineX1
        {
            set { _nlinex1 = value; }
            get { return _nlinex1; }
        }

        /// <summary>
        /// 未登录图层线段端点1的纵坐标
        /// </summary>
        public double NLineY1
        {
            set { _nliney1 = value; }
            get { return _nliney1; }
        }

        /// <summary>
        /// 未登录图层线段端点2的横坐标
        /// </summary>
        public double NLineX2
        {
            set { _nlinex2 = value; }
            get { return _nlinex2; }
        }

        /// <summary>
        /// 未登录图层线段端点2的纵坐标
        /// </summary>
        public double NLineY2
        {
            set { _nliney2 = value; }
            get { return _nliney2; }
        }
        #endregion
    }
}
