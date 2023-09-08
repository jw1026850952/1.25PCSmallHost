using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCSmallHost.DB.Model;
using PCSmallHost.DB.DAL;
using System.Data;

namespace PCSmallHost.DB.BLL
{
    public class CGblSetting
    {
        private readonly GblSetting dal = new GblSetting();
        public CGblSetting()
        { }
        #region  BasicMethod

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<GblSettingInfo> GetAll()
        {
            return dal.GetAll();
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(GblSettingInfo infoGblSetting)
        {
            return dal.Update(infoGblSetting);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstGblSetting"></param>
        /// <returns></returns>
        public int Save(List<GblSettingInfo> LstGblSetting)
        {
            return dal.Save(LstGblSetting);
        }

        /// <summary>
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<GblSettingInfo> LstGblSetting)
        {
            return dal.RestoreOrBackUpData(LstGblSetting);
        }

        /// <summary>
        /// 删除全部数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            return dal.DeleteAll();
        }

        #endregion  BasicMethod
    }
}
