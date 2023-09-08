using System;
using System.Data;
using System.Collections.Generic;
using PCSmallHost.DB.Model;
using PCSmallHost.DB.DAL;
namespace PCSmallHost.DB.BLL
{
    /// <summary>
    /// CFireAlarmType
    /// </summary>
    public partial class CFireAlarmType
    {
        private readonly FireAlarmType dal = new FireAlarmType();
        public CFireAlarmType()
        { }
        #region  BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(FireAlarmTypeInfo infoFireAlarmType)
        {
            return dal.Add(infoFireAlarmType);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(FireAlarmTypeInfo infoFireAlarmType)
        {
            return dal.Update(infoFireAlarmType);
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
        /// 保存多条数据
        /// </summary>
        /// <param name="LstFireAlarmType"></param>
        /// <returns></returns>
        public int Save(List<FireAlarmTypeInfo> LstFireAlarmType)
        {
            return dal.Save(LstFireAlarmType);
        }

        /// <summary>
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<FireAlarmTypeInfo> LstFireAlarmType)
        {
            return dal.RestoreOrBackUpData(LstFireAlarmType);
        }
      
        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<FireAlarmTypeInfo> GetAll()
        {
            return dal.GetAll();           
        }        

        #endregion  BasicMethod       
    }
}

