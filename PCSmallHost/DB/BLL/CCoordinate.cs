using PCSmallHost.DB.DAL;
using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.BLL
{
    public partial class CCoordinate
    {
        private readonly Coordinate dal = new Coordinate(); 

        public CCoordinate() { }

        #region BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="infoCoordinate"></param>
        /// <returns></returns>
        public int Add(CoordinateInfo infoCoordinate)
        {
            return dal.Add(infoCoordinate);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoCoordinate"></param>
        /// <returns></returns>
        public bool Update(CoordinateInfo infoCoordinate)
        {
            return dal.Update(infoCoordinate);
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
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool Delete(int ID)
        {
            return dal.Delete(ID);
        }

        /// <summary>
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <param name="LstCoordinate"></param>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<CoordinateInfo> LstCoordinate)
        {
            return dal.RestoreOrBackUpData(LstCoordinate);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstCoordinate"></param>
        /// <returns></returns>
        public int Save(List<CoordinateInfo> LstCoordinate)
        {
            return dal.Save(LstCoordinate);
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public List<CoordinateInfo> GetAll()
        {
            return dal.GetAll();
        }
        #endregion
    }
}
