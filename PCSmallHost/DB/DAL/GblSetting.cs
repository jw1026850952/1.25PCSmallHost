using PCSmallHost.DB.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCSmallHost.DB.Model;
using System.Data.SQLite;
using Newtonsoft.Json;

namespace PCSmallHost.DB.DAL
{
    public class GblSetting
    {
        #region  BasicMethod
        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<GblSettingInfo> GetAll()
        {
            string strSql = "select * From GblSetting Order By ID";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<GblSettingInfo>>(strJsonString);
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(GblSettingInfo infoGblSetting)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update GblSetting set ");
            strSql.Append("Key=@Key,");
            strSql.Append("SetValue=@SetValue");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Key", DbType.String),
                    new SQLiteParameter("@SetValue", DbType.String),
                    new SQLiteParameter("@ID", DbType.Int32,8)};
            parameters[0].Value = infoGblSetting.Key;
            parameters[1].Value = infoGblSetting.SetValue;
            parameters[2].Value = infoGblSetting.ID;

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

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstGblSetting"></param>
        /// <returns></returns>
        public int Save(List<GblSettingInfo> LstGblSetting)
        {
            List<String> SQLStringList = new List<string>();
            foreach (GblSettingInfo infoGblSetting in LstGblSetting)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update GblSetting set ");
                strSql.Append(string.Format("Key='{0}',", infoGblSetting.Key));
                strSql.Append(string.Format("SetValue='{0}'", infoGblSetting.SetValue));
                strSql.Append(string.Format(" where ID={0};", infoGblSetting.ID));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<GblSettingInfo> LstGblSetting)
        {
            List<String> SQLStringList = new List<string>();
            foreach (GblSettingInfo infoGblSetting in LstGblSetting)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(string.Format("insert into GblSetting values("));
                strSql.Append(string.Format("{0},'{1}','{2}');", infoGblSetting.ID, infoGblSetting.Key, infoGblSetting.SetValue));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 删除全部数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            string strSql = "delete from GblSetting ";
            int rows = DBHelperSQLite.ExecuteSql(strSql);
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion  BasicMethod
    }
}
