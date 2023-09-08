using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.BLL
{
    public class CEscapeRoutes
    {
        private readonly EscapeRoutes dal = new EscapeRoutes();

        public CEscapeRoutes()
        { }

        #region BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="infoEscapeRoutes"></param>
        /// <returns></returns>
        public int Add(EscapeRoutesInfo infoEscapeRoutes)
        {
            return dal.Add(infoEscapeRoutes);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoEscapeRoutes"></param>
        /// <returns></returns>
        public bool Update(EscapeRoutesInfo infoEscapeRoutes)
        {
            return dal.Update(infoEscapeRoutes);
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
        /// 删除全部数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            return dal.DeleteAll();
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public List<EscapeRoutesInfo> GetAll()
        {
            return dal.GetAll();
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstEscapeRoutes"></param>
        /// <returns></returns>
        public int Save(List<EscapeRoutesInfo> LstEscapeRoutes)
        {
            return dal.Save(LstEscapeRoutes);
        }
        #endregion
    }
}
