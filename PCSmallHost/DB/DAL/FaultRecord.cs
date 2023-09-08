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
    public class FaultRecord
    {
        public FaultRecord()
        { }

        #region BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="infoFaultRecord"></param>
        /// <returns></returns>
        public int Add(FaultRecordInfo infoFaultRecord)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into FaultRecord(");
            strSql.Append("Subject,ChildSubject,FaultType,Fault)");
            strSql.Append("values(");
            strSql.Append("@Subject,@ChildSubject,@FaultType,@Fault)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters ={
                new SQLiteParameter("@Subject",DbType.String),
                new SQLiteParameter("@ChildSubject",DbType.String),
                new SQLiteParameter("@FaultType",DbType.String),
                new SQLiteParameter("@Fault",DbType.String)};
            parameters[0].Value = infoFaultRecord.Subject;
            parameters[1].Value = infoFaultRecord.ChildSubject;
            parameters[2].Value = infoFaultRecord.FaultType;
            parameters[3].Value = infoFaultRecord.Fault;

            object obj = DBHelperSQLite.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                infoFaultRecord.ID = Convert.ToInt32(obj);
                return infoFaultRecord.ID;
            }
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="LstFaultRecord"></param>
        /// <returns></returns>
        public List<int> AddAll(List<FaultRecordInfo> LstFaultRecord)
        {
            List<string> SQLStringList = new List<string>();
            foreach (FaultRecordInfo infoFaultRecord in LstFaultRecord)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update FaultRecord set ");
                strSql.Append(string.Format("Subject='{0}',", infoFaultRecord.Subject));
                strSql.Append(string.Format("ChildSubject='{0}',", infoFaultRecord.ChildSubject));
                strSql.Append(string.Format("FaultType='{0}',", infoFaultRecord.FaultType));
                strSql.Append(string.Format("Fault='{0}'", infoFaultRecord.Fault));
                strSql.Append(string.Format(" where ID={0}", infoFaultRecord.ID));
                SQLStringList.Add(strSql.ToString());
            }
            List<int> IDs = DBHelperSQLite.AExecuteSqlTran(SQLStringList);

            if (IDs == null)
            {
                return null;
            }
            else
            {
                for (int i = 0; i < LstFaultRecord.Count; i++)
                {
                    LstFaultRecord[i].ID = IDs[i];
                }
                return IDs;
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoHistoricalEvent"></param>
        /// <returns></returns>
        public bool Update(FaultRecordInfo infoFaultRecord)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update FaultRecord(");
            strSql.Append("Subject=@Subject,");
            strSql.Append("ChildSubject=@ChildSubject,");
            strSql.Append("FaultType=@FaultType,");
            strSql.Append("Fault=@Fault");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters ={
                new SQLiteParameter("@Subject",DbType.String),
                new SQLiteParameter("@ChildSubject",DbType.String),
                new SQLiteParameter("@FaultType",DbType.String),
                new SQLiteParameter("@Fault",DbType.String),
                new SQLiteParameter("@ID",DbType.Int32,8)};
            parameters[0].Value = infoFaultRecord.Subject;
            parameters[1].Value = infoFaultRecord.ChildSubject;
            parameters[2].Value = infoFaultRecord.FaultType;
            parameters[3].Value = infoFaultRecord.Fault;
            parameters[4].Value = infoFaultRecord.ID;

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
        public int RestoreOrBackUpData(List<FaultRecordInfo> LstFaultRecord)
        {
            List<string> SQLStringList = new List<string>();
            foreach (FaultRecordInfo infoFaultRecord in LstFaultRecord)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(string.Format("insert into FaultRecord values("));
                strSql.Append(string.Format("{0},'{1}','{2}','{3}','{4}');", infoFaultRecord.ID, infoFaultRecord.Subject, infoFaultRecord.ChildSubject,infoFaultRecord.FaultType, infoFaultRecord.Fault));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 更改多条数据
        /// </summary>
        /// <param name="LstHistoricalEvent"></param>
        /// <returns></returns>
        public int Save(List<FaultRecordInfo> LstFaultRecord)
        {
            List<string> SQLStringList = new List<string>();
            foreach (FaultRecordInfo infoFaultRecord in LstFaultRecord)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update FaultRecord set ");
                strSql.Append(string.Format("Subject='{0}',", infoFaultRecord.Subject));
                strSql.Append(string.Format("ChildSubject='{0}',", infoFaultRecord.ChildSubject));
                strSql.Append(string.Format("FaultType='{0}',", infoFaultRecord.FaultType));
                strSql.Append(string.Format("Fault='{0}'", infoFaultRecord.Fault));
                strSql.Append(string.Format(" where ID={0}", infoFaultRecord.ID));
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
            string strSql = "delete from FaultRecord;";
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
            strSql.Append("delete from FaultRecord ");
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
        public List<FaultRecordInfo> GetAll()
        {
            string strSql = "select * from FaultRecord;";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<FaultRecordInfo>>(strJsonString);
        }
        #endregion
    }
}
