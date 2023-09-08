using System;
using System.Data;
using System.Text;
using System.Data.SQLite;
using PCSmallHost.DB.Model;
using PCSmallHost.DB.DBUtility;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace PCSmallHost.DB.DAL
{
    /// <summary>
    /// 数据访问类:FireAlarmType
    /// </summary>
    public partial class FireAlarmType
    {
        public FireAlarmType()
        { }
        #region  BasicMethod       

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(FireAlarmTypeInfo infoFireAlarmType)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into FireAlarmType(");
            strSql.Append("FireAlarmCode,FireAlarmName,IsCurrentFireAlarm)");
            strSql.Append(" values (");
            strSql.Append("@FireAlarmCode,@FireAlarmName,@IsCurrentFireAlarm)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@FireAlarmCode", DbType.String),
                    new SQLiteParameter("@FireAlarmName", DbType.String),
                    new SQLiteParameter("@IsCurrentFireAlarm", DbType.Int32,8)};
            parameters[0].Value = infoFireAlarmType.FireAlarmCode;
            parameters[1].Value = infoFireAlarmType.FireAlarmName;
            parameters[2].Value = infoFireAlarmType.IsCurrentFireAlarm;

            object obj = DBHelperSQLite.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(obj);
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(FireAlarmTypeInfo infoFireAlarmType)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update FireAlarmType set ");
            strSql.Append("FireAlarmCode=@FireAlarmCode,");
            strSql.Append("FireAlarmName=@FireAlarmName,");
            strSql.Append("IsCurrentFireAlarm=@IsCurrentFireAlarm");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@FireAlarmCode", DbType.String),
                    new SQLiteParameter("@FireAlarmName", DbType.String),
                    new SQLiteParameter("@IsCurrentFireAlarm", DbType.Int32,8),
                    new SQLiteParameter("@ID", DbType.Int32,8)};
            parameters[0].Value = infoFireAlarmType.FireAlarmCode;
            parameters[1].Value = infoFireAlarmType.FireAlarmName;
            parameters[2].Value = infoFireAlarmType.IsCurrentFireAlarm;
            parameters[3].Value = infoFireAlarmType.ID;

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
        /// <param name="LstFireAlarmType"></param>
        /// <returns></returns>
        public int Save(List<FireAlarmTypeInfo> LstFireAlarmType)
        {
            List<String> SQLStringList = new List<string>();
            foreach (FireAlarmTypeInfo infoFireAlarmType in LstFireAlarmType)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update FireAlarmType set ");
                strSql.Append(string.Format("FireAlarmCode='{0}',", infoFireAlarmType.FireAlarmCode));
                strSql.Append(string.Format("FireAlarmName='{0}',", infoFireAlarmType.FireAlarmName));
                strSql.Append(string.Format("IsCurrentFireAlarm={0}", infoFireAlarmType.IsCurrentFireAlarm));
                strSql.Append(string.Format(" where ID={0};", infoFireAlarmType.ID));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<FireAlarmTypeInfo> LstFireAlarmType)
        {
            List<String> SQLStringList = new List<string>();
            foreach (FireAlarmTypeInfo infoFireAlarmType in LstFireAlarmType)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(string.Format("insert into FireAlarmType values("));
                strSql.Append(string.Format("{0},'{1}','{2}',{3});", infoFireAlarmType.ID, infoFireAlarmType.FireAlarmCode, infoFireAlarmType.FireAlarmName, infoFireAlarmType.IsCurrentFireAlarm));
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
            string strSql = "delete from FireAlarmType ";
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

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<FireAlarmTypeInfo> GetAll()
        {
            string strSql = "select * FROM FireAlarmType";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<FireAlarmTypeInfo>>(strJsonString);
        }

        #endregion  BasicMethod      
    }
}

