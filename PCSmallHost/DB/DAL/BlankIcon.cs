using Newtonsoft.Json;
using PCSmallHost.DB.DBUtility;
using PCSmallHost.DB.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCSmallHost.DB.DAL
{
    public partial class BlankIcon
    {
        public BlankIcon() { }

        #region BasicMethod
        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="infoBlankIcon"></param>
        /// <returns></returns>
        public int Add(BlankIconInfo infoBlankIcon)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into BlankIcon(");
            strSql.Append("Type,DisBoxCode,RtnDirection,EscapeLineID)");
            strSql.Append(" values (");
            strSql.Append("@Type,@DisBoxCode,@RtnDirection,@EscapeLineID)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters =
            {
                new SQLiteParameter("@Type",DbType.String),
                new SQLiteParameter("@DisBoxCode",DbType.String),
                new SQLiteParameter("@RtnDirection",DbType.Decimal,4),
                new SQLiteParameter("@EscapeLineID",DbType.Int32,8)
            };
            parameters[0].Value = infoBlankIcon.Type;
            parameters[1].Value = infoBlankIcon.DisboxCode;
            parameters[2].Value = infoBlankIcon.RtnDirection;
            parameters[3].Value = infoBlankIcon.EscapeLineID;

            object obj=DBHelperSQLite.GetSingle(strSql.ToString(),parameters);
            if(obj == null)
            {
                return 0;
            }
            else
            {
                infoBlankIcon.ID = Convert.ToInt32(obj);
                return infoBlankIcon.ID;
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoBlankIcon"></param>
        /// <returns></returns>
        public bool Update(BlankIconInfo infoBlankIcon)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update BlankIcon set ");
            strSql.Append("Type=@Type,");
            strSql.Append("DisBoxCode=@DisBoxCode,");
            strSql.Append("RtnDirection=@RtnDirection,");
            strSql.Append("EscapeLineID=@EscapeLineID");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters =
            {
                new SQLiteParameter("@Type",DbType.String),
                new SQLiteParameter("@DisBocCode",DbType.String),
                new SQLiteParameter("@RtnDirection",DbType.Decimal,4),
                new SQLiteParameter("@EscapeLineID",DbType.Int32,8),
                new SQLiteParameter("@ID",DbType.Int32,8)
            };
            parameters[0].Value = infoBlankIcon.Type;
            parameters[1].Value = infoBlankIcon.DisboxCode;
            parameters[2].Value = infoBlankIcon.RtnDirection;
            parameters[3].Value = infoBlankIcon.EscapeLineID;
            parameters[4].Value = infoBlankIcon.ID;

            int rows=DBHelperSQLite.ExecuteSql(strSql.ToString(), parameters);
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
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <param name="LstBlankIcon"></param>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<BlankIconInfo> LstBlankIcon)
        {
            List<string> SQLStringList = new List<string>();
            foreach (BlankIconInfo infoBlankIcon in LstBlankIcon)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(string.Format("insert into BlankIcon values("));
                strSql.Append(string.Format("{0},'{1}','{2}',{3},{4});", infoBlankIcon.ID, infoBlankIcon.Type, infoBlankIcon.DisboxCode, infoBlankIcon.RtnDirection, infoBlankIcon.EscapeLineID));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="lstBlankIcon"></param>
        /// <returns></returns>
        public int Save(List<BlankIconInfo> lstBlankIcon)
        {
            List<string> SQLStringList = new List<string>();
            foreach(BlankIconInfo infoBlankIcon in lstBlankIcon)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update BlankIcon set ");
                strSql.Append(string.Format("Type='{0}',", infoBlankIcon.Type));
                strSql.Append(string.Format("DisBoxCode='{0}',", infoBlankIcon.DisboxCode));
                strSql.Append(string.Format("RtnDirection='{0}',", infoBlankIcon.RtnDirection));
                strSql.Append(string.Format("EscapeLineID='{0}'", infoBlankIcon.EscapeLineID));
                strSql.Append(string.Format(" where ID={0};", infoBlankIcon.ID));
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
            string strSql = "delete from BlankIcon;";
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
        /// 删除一条数据
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool Delete(int ID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from BlankIcon ");
            strSql.Append(" where ID=@ID;");
            SQLiteParameter[] parameters =
            {
                new SQLiteParameter("@ID",DbType.Int32,4)
            };
            parameters[0].Value = ID;
            int rows = DBHelperSQLite.ExecuteSql(strSql.ToString(), parameters);
            if(rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public List<BlankIconInfo> GetAll()
        {
            string strSql = "select * from BlankIcon;";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString =JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<BlankIconInfo>>(strJsonString);
        }
        #endregion

    }
}
