using System;
using System.Data;
using System.Collections.Generic;
using PCSmallHost.DB.Model;
using PCSmallHost.DB.DAL;
namespace PCSmallHost.DB.BLL
{
    /// <summary>
    /// CFireAlarmPartitionSet
    /// </summary>
    public partial class CFireAlarmPartitionSet
    {
        private readonly FireAlarmPartitionSet dal = new FireAlarmPartitionSet();
        public CFireAlarmPartitionSet()
        { }
        #region  BasicMethod
        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(FireAlarmPartitionSetInfo infoFireAlarmPartitionSet)
        {
            return dal.Add(infoFireAlarmPartitionSet);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(FireAlarmPartitionSetInfo infoFireAlarmPartitionSet)
        {
            return dal.Update(infoFireAlarmPartitionSet);
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
        public List<FireAlarmPartitionSetInfo> GetAll()
        {
            return dal.GetAll();          
        }
        
        /// <summary>
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet)
        {
            return dal.RestoreOrBackUpData(LstFireAlarmPartitionSet);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <returns></returns>
        public int Save(List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet)
        {
            return dal.Save(LstFireAlarmPartitionSet);
        }

        #endregion  BasicMethod      
    }
}

