using System;
using System.Data;
using System.Collections.Generic;
using PCSmallHost.DB.Model;
using PCSmallHost.DB.DAL
;
namespace PCSmallHost.DB.BLL
{
    /// <summary>
    /// CLight
    /// </summary>
    public partial class CLight
    {
        private readonly Light dal = new Light();
        public CLight()
        { }
        #region  BasicMethod
        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(LightInfo infoLight)
        {
            return dal.Add(infoLight);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(LightInfo infoLight)
        {
            return dal.Update(infoLight);
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
        /// 获得数据列表
        /// </summary>
        public List<LightInfo> GetAll()
        {
            return dal.GetAll();            
        }

        /// <summary>
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<LightInfo> LstLight)
        {
            return dal.RestoreOrBackUpData(LstLight);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <returns></returns>
        public int Save(List<LightInfo> LstLight)
        {
            return dal.Save(LstLight);
        }

        #endregion  BasicMethod      
    }
}

