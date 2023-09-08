using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    public class EscapeRoutesInfo
    {
        public EscapeRoutesInfo()
        { }
        #region Model
        private int _id;
        private int _startingpoint;
        private int _endpoint;
        private int _turnindex;

        /// <summary>
        /// 
        /// </summary>
        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 是否是路线起点
        /// </summary>
        public int StartingPoint
        {
            set { _startingpoint = value; }
            get { return _startingpoint; }
        }

        /// <summary>
        /// 是否是终点
        /// </summary>
        public int EndPoint
        {
            set { _endpoint = value; }
            get { return _endpoint; }
        }

        /// <summary>
        /// 标点在图纸上的索引
        /// </summary>
        public int TurnIndex
        {
            set { _turnindex = value; }
            get { return _turnindex; }
        }

        #endregion
    }
}
