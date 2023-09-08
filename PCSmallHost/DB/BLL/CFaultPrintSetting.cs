using PCSmallHost.DB.DAL;
using PCSmallHost.DB.Model;
using System.Collections.Generic;

namespace PCSmallHost.DB.BLL
{
    public class CFaultPrintSetting
    {
        private readonly FaultPrintSetting dal = new FaultPrintSetting();

        public bool Update(FaultPrintSettingInfo faultPrintSetting)
        {
            return dal.Update(faultPrintSetting);
        }

        public List<FaultPrintSettingInfo> GetAll()
        {
            return dal.GetAll();
        }
    }
}
