using System;
using System.Data;
using System.Text;
using System.Data.SQLite;
using PCSmallHost.DB.DBUtility;
using PCSmallHost.DB.Model;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace PCSmallHost.DB.DAL
{
    /// <summary>
    /// 数据访问类:FireAlarmPartitionSet
    /// </summary>
    public partial class FireAlarmPartitionSet
    {
        public FireAlarmPartitionSet()
        { }
        #region  BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(FireAlarmPartitionSetInfo infoFireAlarmPartitionSet)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into FireAlarmPartitionSet(");
            strSql.Append("PlanPartition,Floor,MainboardCircuit,DeviceClass,LowDeviceRange,HighDeviceRange)");
            strSql.Append(" values (");
            strSql.Append("@PlanPartition,@Floor,@MainboardCircuit,@DeviceClass,@LowDeviceRange,@HighDeviceRange)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@PlanPartition", DbType.Int32,8),
                    new SQLiteParameter("@Floor", DbType.Int32,8),
                    new SQLiteParameter("@MainboardCircuit", DbType.Int32,8),
                    new SQLiteParameter("@DeviceClass", DbType.String,20),
                    new SQLiteParameter("@LowDeviceRange", DbType.Int32,8),
                    new SQLiteParameter("@HighDeviceRange", DbType.Int32,8)};
            parameters[0].Value = infoFireAlarmPartitionSet.PlanPartition;
            parameters[1].Value = infoFireAlarmPartitionSet.Floor;
            parameters[2].Value = infoFireAlarmPartitionSet.MainBoardCircuit;
            parameters[3].Value = infoFireAlarmPartitionSet.DeviceClass;
            parameters[4].Value = infoFireAlarmPartitionSet.LowDeviceRange;
            parameters[5].Value = infoFireAlarmPartitionSet.HighDeviceRange;

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
        public bool Update(FireAlarmPartitionSetInfo infoFireAlarmPartitionSet)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update FireAlarmPartitionSet set ");
            strSql.Append("PlanPartition=@PlanPartition,");
            strSql.Append("Floor=@Floor,");
            strSql.Append("MainboardCircuit=@MainboardCircuit,");
            strSql.Append("DeviceClass=@DeviceClass,");
            strSql.Append("LowDeviceRange=@LowDeviceRange,");
            strSql.Append("HighDeviceRange=@HighDeviceRange");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@PlanPartition", DbType.Int32,8),
                    new SQLiteParameter("@Floor", DbType.Int32,8),
                    new SQLiteParameter("@MainboardCircuit", DbType.Int32,8),
                    new SQLiteParameter("@DeviceClass", DbType.String,20),
                    new SQLiteParameter("@LowDeviceRange", DbType.Int32,8),
                    new SQLiteParameter("@HighDeviceRange", DbType.Int32,8),
                    new SQLiteParameter("@ID", DbType.Int32,8)};
            parameters[0].Value = infoFireAlarmPartitionSet.PlanPartition;
            parameters[1].Value = infoFireAlarmPartitionSet.Floor;
            parameters[2].Value = infoFireAlarmPartitionSet.MainBoardCircuit;
            parameters[3].Value = infoFireAlarmPartitionSet.DeviceClass;
            parameters[4].Value = infoFireAlarmPartitionSet.LowDeviceRange;
            parameters[5].Value = infoFireAlarmPartitionSet.HighDeviceRange;
            parameters[6].Value = infoFireAlarmPartitionSet.ID;

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
        /// <returns></returns>
        public int RestoreOrBackUpData(List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet)
        {
            List<String> SQLStringList = new List<string>();
            foreach (FireAlarmPartitionSetInfo infoFireAlarmPartitionSet in LstFireAlarmPartitionSet)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(string.Format("insert into FireAlarmPartitionSet values("));
                strSql.Append(string.Format("{0},{1},{2},{3},'{4}',{5},{6});", infoFireAlarmPartitionSet.ID, infoFireAlarmPartitionSet.PlanPartition,
                    infoFireAlarmPartitionSet.Floor, infoFireAlarmPartitionSet.MainBoardCircuit, infoFireAlarmPartitionSet.DeviceClass, infoFireAlarmPartitionSet.LowDeviceRange, infoFireAlarmPartitionSet.HighDeviceRange));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <returns></returns>
        public int Save(List<FireAlarmPartitionSetInfo> LstFireAlarmPartitionSet)
        {
            List<String> SQLStringList = new List<string>();
            foreach (FireAlarmPartitionSetInfo infoFireAlarmPartitionSet in LstFireAlarmPartitionSet)
            {
                if (infoFireAlarmPartitionSet.PlanPartition == 0 && infoFireAlarmPartitionSet.MainBoardCircuit == 0 & infoFireAlarmPartitionSet.Floor == 0 && infoFireAlarmPartitionSet.LowDeviceRange == 0 && infoFireAlarmPartitionSet.HighDeviceRange == 0 && infoFireAlarmPartitionSet.DeviceClass == null
                    )
                {
                    continue;
                }
                StringBuilder strSql = new StringBuilder();
                if (infoFireAlarmPartitionSet.ID == 0)
                {
                    strSql.Append(string.Format("insert into FireAlarmPartitionSet values("));
                    strSql.Append(string.Format("(select (max(ID) + 1) from FireAlarmPartitionSet),"));
                    strSql.Append(string.Format("{0},{1},{2},'{3}',{4},{5});", infoFireAlarmPartitionSet.PlanPartition,
                        infoFireAlarmPartitionSet.Floor, infoFireAlarmPartitionSet.MainBoardCircuit, infoFireAlarmPartitionSet.DeviceClass, infoFireAlarmPartitionSet.LowDeviceRange, infoFireAlarmPartitionSet.HighDeviceRange));
                }
                else
                {
                    strSql.Append(string.Format("update FireAlarmPartitionSet set "));
                    strSql.Append(string.Format("PlanPartition= {0},", infoFireAlarmPartitionSet.PlanPartition));
                    strSql.Append(string.Format("Floor= {0},", infoFireAlarmPartitionSet.Floor));
                    strSql.Append(string.Format("MainboardCircuit= {0},", infoFireAlarmPartitionSet.MainBoardCircuit));
                    strSql.Append(string.Format("DeviceClass='{0}',", infoFireAlarmPartitionSet.DeviceClass));
                    strSql.Append(string.Format("LowDeviceRange= {0},", infoFireAlarmPartitionSet.LowDeviceRange));
                    strSql.Append(string.Format("HighDeviceRange={0}", infoFireAlarmPartitionSet.HighDeviceRange));
                    strSql.Append(string.Format(" where ID={0};", infoFireAlarmPartitionSet.ID));
                }
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool Delete(int ID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from FireAlarmPartitionSet ");
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
            string strSql = "delete from FireAlarmPartitionSet ";
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
        public List<FireAlarmPartitionSetInfo> GetAll()
        {
            string strSql = "select * From FireAlarmPartitionSet";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<FireAlarmPartitionSetInfo>>(strJsonString);
        }

        #endregion  BasicMethod       
    }
}

