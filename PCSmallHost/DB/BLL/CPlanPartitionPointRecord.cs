using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCSmallHost.DB.Model;
using PCSmallHost.DB.DAL;

namespace PCSmallHost.DB.BLL
{
    public class CPlanPartitionPointRecord
    {
        private readonly PlanPartitionPointRecord dal=new PlanPartitionPointRecord();

		public CPlanPartitionPointRecord()
		{}

		#region  BasicMethod
		/// <summary>
		/// 增加一条数据
		/// </summary>
        public int Add(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord)
		{
            return dal.Add(infoPlanPartitionPointRecord);
		}

		/// <summary>
		/// 更新一条数据
		/// </summary>
        public bool Update(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord)
		{
            return dal.Update(infoPlanPartitionPointRecord);
		}

		/// <summary>
		/// 删除一条数据
		/// </summary>
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
        /// 获得数据列表
        /// </summary>
        public List<PlanPartitionPointRecordInfo> GetAll()
        {
            return dal.GetAll();
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstPlanPartitionPointRecord"></param>
        /// <returns></returns>
        public int Save(List<PlanPartitionPointRecordInfo> LstPlanPartitionPointRecord)
        {
            return dal.Save(LstPlanPartitionPointRecord);
        }
		
		#endregion  BasicMethod
    }
}
