using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCSmallHost.DB.Model;
using System.Data.SQLite;
using System.Data;
using PCSmallHost.DB.DBUtility;
using Newtonsoft.Json;

namespace PCSmallHost.DB.DAL
{
    public class PlanPartitionPointRecord
    {
        public PlanPartitionPointRecord()
        { }
        #region  BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into PlanPartitionPointRecord(");
            strSql.Append("PlanPartition,EscapeLineID)");
            strSql.Append(" values (");
            strSql.Append("@PlanPartition,@EscapeLineID)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@PlanPartition", DbType.Int32,8),
                    new SQLiteParameter("@EscapeLineID",DbType.Int32,8)};
            parameters[0].Value = infoPlanPartitionPointRecord.PlanPartition;
            parameters[1].Value = infoPlanPartitionPointRecord.EscapeLineID == null ? -1 : infoPlanPartitionPointRecord.EscapeLineID;

            object obj = DBHelperSQLite.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                infoPlanPartitionPointRecord.ID = Convert.ToInt32(obj);
                return infoPlanPartitionPointRecord.ID;
            }
        }
        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(PlanPartitionPointRecordInfo infoPlanPartitionPointRecord)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update PlanPartitionPointRecord set ");
            strSql.Append("PlanPartition=@PlanPartition,");
            strSql.Append("EscapeLineID=@EscapeLineID");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@PlanPartition", DbType.Int32,8),
                    new SQLiteParameter("@EscapeLineID",DbType.Int32,8),
                    new SQLiteParameter("@ID", DbType.Int32,8)};
            parameters[0].Value = infoPlanPartitionPointRecord.PlanPartition;
            parameters[1].Value = infoPlanPartitionPointRecord.EscapeLineID == null ? -1 : infoPlanPartitionPointRecord.EscapeLineID;
            parameters[2].Value = infoPlanPartitionPointRecord.ID;

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
        public bool Delete(int ID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from PlanPartitionPointRecord ");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@ID", DbType.Int32,4)
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
        /// 删除全部数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            string strSql = "delete from PlanPartitionPointRecord;";
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
        public List<PlanPartitionPointRecordInfo> GetAll()
        {
            string strSql = "select * From PlanPartitionPointRecord";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<PlanPartitionPointRecordInfo>>(strJsonString);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstPlanPartitionPointRecord"></param>
        /// <returns></returns>
        public int Save(List<PlanPartitionPointRecordInfo> LstPlanPartitionPointRecord)
        {
            List<String> SQLStringList = new List<string>();
            foreach (PlanPartitionPointRecordInfo infoPlanPartitionPointRecord in LstPlanPartitionPointRecord)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update PlanPartitionPointRecord set ");
                strSql.Append(string.Format("ID={0},", infoPlanPartitionPointRecord.ID));
                strSql.Append(string.Format("PlanPartition={0},", infoPlanPartitionPointRecord.PlanPartition));
                strSql.Append(string.Format("EscapeLineID={0}", infoPlanPartitionPointRecord.EscapeLineID == null ? -1 : infoPlanPartitionPointRecord.EscapeLineID));
                strSql.Append(string.Format(" where ID={0};", infoPlanPartitionPointRecord.ID));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        #endregion  BasicMethod
    }
}
