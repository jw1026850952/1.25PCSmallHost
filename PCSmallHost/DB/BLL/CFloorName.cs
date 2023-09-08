using PCSmallHost.DB.DAL;
using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.BLL
{
    public class CFloorName
    {
        private readonly FloorName dal = new FloorName();
        public CFloorName()
        { }
        #region  BasicMethod
        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(FloorNameInfo infoFloorName)
        {
            return dal.Add(infoFloorName);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(FloorNameInfo infoFloorName)
        {
            return dal.Update(infoFloorName);
        }

        /// <summary>
        /// 删除全部数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            return dal.DeleteAll();
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool Delete(int ID)
        {
            return dal.Delete(ID);
        }

        ///// <summary>
        ///// 备份数据或者恢复备份数据
        ///// </summary>
        ///// <returns></returns>
        //public int RestoreOrBackUpData(List<FloorNameInfo> LstFloorName)
        //{
        //    return dal.RestoreOrBackUpData(LstFloorName);
        //}

        /// <summary>
        /// 保存多条数据
        /// </summary>
        public int Save(List<FloorNameInfo> LstFloorName)
        {
            return dal.Save(LstFloorName);
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<FloorNameInfo> GetAll()
        {
            return dal.GetAll();
        }

        #endregion  BasicMethod       
    }
}
