using System;
using System.Data;
using System.Collections.Generic;
using PCSmallHost.DB.Model;
using PCSmallHost.DB.DAL;
namespace PCSmallHost.DB.BLL
{
    /// <summary>
    /// CDistributionBox
    /// </summary>
    public partial class CDistributionBox
    {
        private readonly DistributionBox dal = new DistributionBox();
        public CDistributionBox()
        { }
        #region  BasicMethod
        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(DistributionBoxInfo infoDistributionBox)
        {
            return dal.Add(infoDistributionBox);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(DistributionBoxInfo infoDistributionBox)
        {
            return dal.Update(infoDistributionBox);
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

        /// <summary>
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<DistributionBoxInfo> LstDistributionBox)
        {
            return dal.RestoreOrBackUpData(LstDistributionBox);
        }

         /// <summary>
        /// 保存多条数据
        /// </summary>
        public int Save(List<DistributionBoxInfo> LstDistributionBox)
        {
            return dal.Save(LstDistributionBox);
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<DistributionBoxInfo> GetAll()
        {
            return dal.GetAll();          
        }

        #endregion  BasicMethod       
    }
}

