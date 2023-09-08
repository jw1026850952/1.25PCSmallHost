using Newtonsoft.Json;
using PCSmallHost.DB.DBUtility;
using PCSmallHost.DB.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace PCSmallHost.DB.DAL
{
    public class FaultPrintSetting
    {
        public List<FaultPrintSettingInfo> GetAll()
        {
            string strSql = "select * From FaultPrintSetting;";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<FaultPrintSettingInfo>>(strJsonString);
        }


        public bool Update(FaultPrintSettingInfo settingInfo)
        {

            StringBuilder strSql = new StringBuilder();
            strSql.Append("update FaultPrintSetting set ");
            strSql.Append("IsPrint=@IsPrint");
            strSql.Append(" where Id=@Id");
            SQLiteParameter[] parameters = {
        new SQLiteParameter("@IsPrint", DbType.Int32),
        new SQLiteParameter("@Id",DbType.Int32)
    };
            parameters[0].Value = settingInfo.IsPrint;
            parameters[1].Value = settingInfo.Id;
            int rows = DBHelperSQLite.ExecuteSql(strSql.ToString(), parameters);
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
