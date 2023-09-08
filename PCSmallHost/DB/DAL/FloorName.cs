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
    public class FloorName
    {
        public FloorName()
        { }

        #region BasicMethod

        public int Add(FloorNameInfo infoFloorName)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into FloorName(");
            strSql.Append("FloorName)");
            strSql.Append(" values (");
            strSql.Append("@FloorName)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters =
            {
                new SQLiteParameter("@FloorName",DbType.String)
            };
            parameters[0].Value = infoFloorName.FloorName;

            object obj = DBHelperSQLite.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                infoFloorName.ID = Convert.ToInt32(obj);
                return infoFloorName.ID;
            }
        }

        public bool Update(FloorNameInfo infoFloorName)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update FloorName set ");
            strSql.Append("FloorName=@FloorName");
            strSql.Append(" where ID=@ID");

            SQLiteParameter[] parameters =
            {
                new SQLiteParameter("@FloorName", DbType.String),
                new SQLiteParameter("@ID", DbType.Int32,8)
            };

            parameters[0].Value = infoFloorName.FloorName;
            parameters[1].Value = infoFloorName.ID;

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

        public int Save(List<FloorNameInfo> LstFloorName)
        {
            List<String> SQLStringList = new List<string>();
            foreach (FloorNameInfo infoFloorName in LstFloorName)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update FloorName set ");
                strSql.Append(string.Format("FloorName='{0}'", infoFloorName.FloorName));
                strSql.Append(string.Format(" where ID={0};", infoFloorName.ID));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        public bool DeleteAll()
        {
            string strSql = "delete from FloorName;";
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

        public bool Delete(int ID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from FloorName ");
            strSql.Append(" where ID=@ID;");
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@ID", DbType.Int32,4)};
            parameters[0].Value = ID;

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

        public List<FloorNameInfo> GetAll()
        {
            string strSql = "select * from FloorName;";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<FloorNameInfo>>(strJsonString);
        }
        #endregion
    }
}
