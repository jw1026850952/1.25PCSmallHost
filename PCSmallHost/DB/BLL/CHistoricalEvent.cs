using PCSmallHost.DB.DAL;
using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.BLL
{
    public class CHistoricalEvent
    {
        private readonly HistoricalEvent dal = new HistoricalEvent();
        public CHistoricalEvent()
        { }
        #region BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="infoHistoricalEvent"></param>
        /// <returns></returns>
        public int Add(HistoricalEventInfo infoHistoricalEvent)
        {
            return dal.Add(infoHistoricalEvent);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoHistoricalEvent"></param>
        /// <returns></returns>
        public bool Update(HistoricalEventInfo infoHistoricalEvent)
        {
            return dal.Update(infoHistoricalEvent);
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
        public int RestoreOrBackUpData(List<HistoricalEventInfo> LstHistoricalEvent)
        {
            return dal.RestoreOrBackUpData(LstHistoricalEvent);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="infoHistoricalEvent"></param>
        /// <returns></returns>
        public int Save(List<HistoricalEventInfo> infoHistoricalEvent)
        {
            return dal.Save(infoHistoricalEvent);
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public List<HistoricalEventInfo> GetAll()
        {
            return dal.GetAll();
        }
        #endregion

    }
}
