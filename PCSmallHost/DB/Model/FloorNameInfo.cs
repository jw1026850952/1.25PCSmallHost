using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.Model
{
    public class FloorNameInfo
    {
        public FloorNameInfo()
        {

        }

        #region
        private int _id;
        private string _floorname;

        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }

        /// <summary>
        /// 图层名字
        /// </summary>
        public string FloorName
        {
            set { _floorname = value; }
            get { return _floorname; }
        }
        #endregion
    }
}
