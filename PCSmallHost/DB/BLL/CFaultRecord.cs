using PCSmallHost.DB.DAL;
using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.BLL
{
    public class CFaultRecord
    {
        private readonly FaultRecord dal = new FaultRecord();
        public CFaultRecord()
        { }
        #region BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="infoHistoricalEvent"></param>
        /// <returns></returns>
        public int Add(FaultRecordInfo infoFaultRecord)
        {
            return dal.Add(infoFaultRecord);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoHistoricalEvent"></param>
        /// <returns></returns>
        public bool Update(FaultRecordInfo infoFaultRecord)
        {
            return dal.Update(infoFaultRecord);
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
        /// <param name="LstHistoricalEvent"></param>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<FaultRecordInfo> LstFaultRecord)
        {
            return dal.RestoreOrBackUpData(LstFaultRecord);
        }

        /// <summary>
        /// 更改多条数据
        /// </summary>
        /// <param name="infoHistoricalEvent"></param>
        /// <returns></returns>
        public int Save(List<FaultRecordInfo> LstFaultRecord)
        {
            return dal.Save(LstFaultRecord);
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public List<FaultRecordInfo> GetAll()
        {
            return dal.GetAll();
        }
        #endregion
    }
}
