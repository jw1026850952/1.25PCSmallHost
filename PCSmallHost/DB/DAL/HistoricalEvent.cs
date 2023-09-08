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
    public class HistoricalEvent
    {
        public HistoricalEvent()
        { }
        #region BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="infoHistoricalEvent"></param>
        /// <returns></returns>
        public int Add(HistoricalEventInfo infoHistoricalEvent)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into HistoricalEvent(");
            strSql.Append("EventTime,EventContent)");
            strSql.Append("values(");
            strSql.Append("@EventTime,@EventContent)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters ={
                new SQLiteParameter("@EventTime",DbType.String),
                new SQLiteParameter("@EventContent",DbType.String)
                                         };
            parameters[0].Value = infoHistoricalEvent.EventTime;
            parameters[1].Value = infoHistoricalEvent.EventContent;

            object obj = DBHelperSQLite.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                infoHistoricalEvent.ID = Convert.ToInt32(obj);
                return infoHistoricalEvent.ID;
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoHistoricalEvent"></param>
        /// <returns></returns>
        public bool Update(HistoricalEventInfo infoHistoricalEvent)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update HistoricalEvent(");
            strSql.Append("EventTime=@EventTime,");
            strSql.Append("EventContent=@EventContent");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters ={
                new SQLiteParameter("@EventTime",DbType.String),
                new SQLiteParameter("@EventContent",DbType.String),
                new SQLiteParameter("@ID",DbType.Int32,8)};
            parameters[0].Value = infoHistoricalEvent.EventTime;
            parameters[1].Value = infoHistoricalEvent.EventContent;
            parameters[2].Value = infoHistoricalEvent.ID;

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
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <param name="LstHistoricalEvent"></param>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<HistoricalEventInfo> LstHistoricalEvent)
        {
            List<string> SQLStringList = new List<string>();
            foreach (HistoricalEventInfo infoHistoricalEvent in LstHistoricalEvent)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(string.Format("insert into HistoricalEvent values("));
                strSql.Append(string.Format("{0},'{1}','{2}');", infoHistoricalEvent.ID, infoHistoricalEvent.EventTime, infoHistoricalEvent.EventContent));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstHistoricalEvent"></param>
        /// <returns></returns>
        public int Save(List<HistoricalEventInfo> LstHistoricalEvent)
        {
            List<string> SQLStringList = new List<string>();
            foreach (HistoricalEventInfo infoHistoricalEvent in LstHistoricalEvent)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update HistoricalEvent set ");
                strSql.Append(string.Format("EventTime='{0}',", infoHistoricalEvent.EventTime));
                strSql.Append(string.Format("EventContent='{0}'", infoHistoricalEvent.EventContent));
                strSql.Append(string.Format(" where ID={0}", infoHistoricalEvent.ID));
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
            string strSql = "delete from HistoricalEvent;";
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
            strSql.Append("delete from HistoricalEvent ");
            strSql.Append(" where ID=@ID;");
            SQLiteParameter[] parameters ={
                new SQLiteParameter("@ID",DbType.Int32,4)
                                         };
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
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public List<HistoricalEventInfo> GetAll()
        {
            string strSql = "select * from HistoricalEvent;";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<HistoricalEventInfo>>(strJsonString);
        }
        #endregion
    }
}
