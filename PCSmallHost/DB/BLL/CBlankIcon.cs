using PCSmallHost.DB.DAL;
using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.BLL
{
    public partial class CBlankIcon
    {
        private readonly BlankIcon dal = new BlankIcon();
        public CBlankIcon() { }

        #region BasicMethod
        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="infoBlankIcon"></param>
        /// <returns></returns>
        public int Add(BlankIconInfo infoBlankIcon)
        {
            return dal.Add(infoBlankIcon);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoBlankIcon"></param>
        /// <returns></returns>
        public bool Update(BlankIconInfo infoBlankIcon)
        {
            return dal.Update(infoBlankIcon);
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
        /// <param name="LstBlankIcon"></param>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<BlankIconInfo> LstBlankIcon)
        {
            return dal.RestoreOrBackUpData(LstBlankIcon);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstBlankIcon"></param>
        /// <returns></returns>
        public int Save(List<BlankIconInfo> LstBlankIcon)
        {
            return dal.Save(LstBlankIcon);
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public List<BlankIconInfo> GetAll()
        {
            return dal.GetAll();
        }
        #endregion
    }
}
