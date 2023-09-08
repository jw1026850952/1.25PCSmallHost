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

namespace PCSmallHost
{
    public class EscapeRoutes
    {
        public EscapeRoutes()
        { }
        #region BasicMethod

        /// <summary>
        /// 增加一条数据 
        /// </summary>
        /// <param name="infoEscapeRoutes"></param>
        /// <returns></returns>
        public int Add(EscapeRoutesInfo infoEscapeRoutes)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into EscapeRoutes(");
            strSql.Append("StartingPoint,EndPoint,TurnIndex)");
            strSql.Append("values(");
            strSql.Append("@StartingPoint,@EndPoint,@TurnIndex)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters ={
                    new SQLiteParameter("@StartingPoint",DbType.Int32,8),
                    new SQLiteParameter("@EndPoint",DbType.Int32,8),
                    new SQLiteParameter("@TurnIndex",DbType.Int32,8)};
            parameters[0].Value = infoEscapeRoutes.StartingPoint;
            parameters[1].Value = infoEscapeRoutes.EndPoint;
            parameters[2].Value = infoEscapeRoutes.TurnIndex;

            object obj = DBHelperSQLite.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                infoEscapeRoutes.ID = Convert.ToInt32(obj);
                return infoEscapeRoutes.ID;
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoEscapeRoutes"></param>
        /// <returns></returns>
        public bool Update(EscapeRoutesInfo infoEscapeRoutes)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update EscapeRoutes set ");
            strSql.Append("StartingPoint=@StartingPoint,");
            strSql.Append("EndPoint=@EndPoint,");
            strSql.Append("TurnIndex=@TurnIndex");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters ={
                    new SQLiteParameter("@StartingPoint",DbType.Int32,8),
                    new SQLiteParameter("@EndPoint",DbType.Int32,8),
                    new SQLiteParameter("@TurnIndex",DbType.Int32,8),
                    new SQLiteParameter("@ID",DbType.Int32,8)};
            parameters[0].Value = infoEscapeRoutes.StartingPoint;
            parameters[1].Value = infoEscapeRoutes.EndPoint;
            parameters[2].Value = infoEscapeRoutes.TurnIndex;
            parameters[3].Value = infoEscapeRoutes.ID;

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
        /// 删除一条数据
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool Delete(int ID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from EscapeRoutes ");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters ={
                    new SQLiteParameter("@ID",DbType.Int32,4)};
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

        /// <summary>
        /// 删除全部数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            string strSql = "delete from EscapeRoutes;";
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
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public List<EscapeRoutesInfo> GetAll()
        {
            string strSql = "select * From EscapeRoutes";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<EscapeRoutesInfo>>(strJsonString);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstEscapeRoutes"></param>
        /// <returns></returns>
        public int Save(List<EscapeRoutesInfo> LstEscapeRoutes)
        {
            List<String> SQLStringList = new List<string>();
            foreach (EscapeRoutesInfo infoEscapeRoutes in LstEscapeRoutes)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update EscapeRoutes set ");
                strSql.Append(string.Format("ID={0},", infoEscapeRoutes.ID));
                strSql.Append(string.Format("StartingPoint={0},", infoEscapeRoutes.StartingPoint));
                strSql.Append(string.Format("EndPoint={0},", infoEscapeRoutes.EndPoint));
                strSql.Append(string.Format("TurnIndex={0}", infoEscapeRoutes.TurnIndex));
                strSql.Append(string.Format(" where ID={0};", infoEscapeRoutes.ID));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }
        #endregion
    }
}
