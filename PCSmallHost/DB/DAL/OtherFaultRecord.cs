using System;
using System.Data;
using System.Text;
using System.Data.SQLite;
using PCSmallHost.DB.DBUtility;
using System.Collections.Generic;
using PCSmallHost.DB.Model;
using Newtonsoft.Json;
namespace PCSmallHost.DB.DAL
{
    /// <summary>
    /// 数据访问类:OtherFaultRecord
    /// </summary>
    public partial class OtherFaultRecord
    {
        public OtherFaultRecord()
        {

        }

        #region  BasicMethod
        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(OtherFaultRecordInfo infoOtherFaultRecord)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update OtherFaultRecord set ");
            strSql.Append("Description=@Description,");
            strSql.Append("IsExist=@IsExist,");
            strSql.Append("Disable=@Disable");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Description", DbType.String),
                    new SQLiteParameter("@IsExist", DbType.Int32,8),
                    new SQLiteParameter("@Disable", DbType.Int32,8),
                    new SQLiteParameter("@ID", DbType.Int32,8)};
            parameters[0].Value = infoOtherFaultRecord.Description;
            parameters[1].Value = infoOtherFaultRecord.IsExist;
            parameters[2].Value = infoOtherFaultRecord.Disable;
            parameters[3].Value = infoOtherFaultRecord.ID;

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
        public int Save(List<OtherFaultRecordInfo> LstOtherFaultRecord)
        {
            List<String> SQLStringList = new List<string>();
            foreach (OtherFaultRecordInfo infoOtherFaultRecord in LstOtherFaultRecord)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update OtherFaultRecord set ");
                strSql.Append(string.Format("Description='{0}',", infoOtherFaultRecord.Description));
                strSql.Append(string.Format("IsExist={0},", infoOtherFaultRecord.IsExist));
                strSql.Append(string.Format("Disable={0}", infoOtherFaultRecord.Disable));
                strSql.Append(string.Format(" where ID={0};", infoOtherFaultRecord.ID));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<OtherFaultRecordInfo> GetAll()
        {
            string strSql = "select * From OtherFaultRecord Order By ID";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<OtherFaultRecordInfo>>(strJsonString);
        }

        #endregion  BasicMethod      
    }
}

