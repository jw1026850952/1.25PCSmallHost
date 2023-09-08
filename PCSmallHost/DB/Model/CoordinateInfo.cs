using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    public partial class CoordinateInfo
    {
        public CoordinateInfo() { }

        #region Model
        private int _id;
        private string _tablename;
        private int _tableid;
        private int _location;
        private double _originx;
        private double _originy;
        private double _nloriginx;
        private double _nloriginy;
        private double _transformx;
        private double _transformy;
        private int _isauth;

        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 对象所在的表名
        /// </summary>
        public string TableName
        {
            set { _tablename = value; }
            get { return _tablename;}
        }

        /// <summary>
        /// 对象在表中的id号
        /// </summary>
        public int TableID
        {
            set { _tableid = value; }
            get { return _tableid;}
        }

        /// <summary>
        /// 对象图标所在的图层号
        /// </summary>
        public int Location
        {
            set { _location = value; }
            get { return _location;}
        }

        public double OriginX
        {
            set { _originx = value; }
            get { return _originx;}
        }

        public double OriginY
        {
            set { _originy = value; }
            get { return _originy;}
        }

        public double NLOriginX
        {
            set { _nloriginx = value; }
            get { return _nloriginx;}
        }

        public double NLOriginY
        {
            set { _nloriginy = value; }
            get { return _nloriginy;}
        }

        public double TransformX
        {
            set { _transformx = value; }
            get { return _transformx; }
        }

        public double TransformY
        {
            set { _transformy = value; }
            get { return _transformy;}
        }

        /// <summary>
        /// 是否封存
        /// </summary>
        public int IsAuth
        {
            set { _isauth = value; }
            get { return _isauth; }
        }
        #endregion
    }
}
