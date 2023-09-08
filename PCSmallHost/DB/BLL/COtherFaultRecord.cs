using System;
using System.Data;
using System.Collections.Generic;
using PCSmallHost.DB.Model;
using PCSmallHost.DB.DAL;
namespace PCSmallHost.DB.BLL
{
    /// <summary>
    /// COtherFaultRecord
    /// </summary>
    public partial class COtherFaultRecord
    {
        private readonly OtherFaultRecord dal = new OtherFaultRecord();
        public COtherFaultRecord()
        { 

        }
        #region  BasicMethod
        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(OtherFaultRecordInfo infoOtherFaultRecord)
        {
            return dal.Update(infoOtherFaultRecord);
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<OtherFaultRecordInfo> GetAll()
        {
            return dal.GetAll();            
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        public int Save(List<OtherFaultRecordInfo> LstOtherFaultRecord)
        {
            return dal.Save(LstOtherFaultRecord);
        }
    
        #endregion  BasicMethod
        #region  ExtensionMethod

        #endregion  ExtensionMethod
    }
}

