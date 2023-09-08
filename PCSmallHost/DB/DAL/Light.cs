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
    /// 数据访问类:Light
    /// </summary>
    public partial class Light
    {
        public Light()
        { }
        #region  BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(LightInfo infoLight)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into Light(");
            strSql.Append("Code,Address,Status,BeginStatus,CurrentState,ErrorTime,Disable,PlanLeft1,PlanLeft2,PlanLeft3,PlanLeft4,PlanLeft5,PlanRight1,PlanRight2,PlanRight3,PlanRight4,PlanRight5,LightClass,DisBoxID,LightIndex,IsEmergency,RtnDirection,EscapeLineID,Shield)");
            strSql.Append(" values (");
            strSql.Append("@Code,@Address,@Status,@BeginStatus,@CurrentState,@ErrorTime,@Disable,@PlanLeft1,@PlanLeft2,@PlanLeft3,@PlanLeft4,@PlanLeft5,@PlanRight1,@PlanRight2,@PlanRight3,@PlanRight4,@PlanRight5,@LightClass,@DisBoxID,@LightIndex,@IsEmergency,@RtnDirection,@EscapeLineID,@Shield)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@Code", DbType.String),
                new SQLiteParameter("@Address", DbType.String),
                new SQLiteParameter("@Status", DbType.Int32,8),
                new SQLiteParameter("@BeginStatus", DbType.Int32,8),
                new SQLiteParameter("@CurrentState",DbType.Int32,8),
                new SQLiteParameter("@ErrorTime", DbType.String),
                new SQLiteParameter("@Disable", DbType.Int32,8),
                new SQLiteParameter("@PlanLeft1", DbType.Int32,8),
                new SQLiteParameter("@PlanLeft2", DbType.Int32,8),
                new SQLiteParameter("@PlanLeft3", DbType.Int32,8),
                new SQLiteParameter("@PlanLeft4", DbType.Int32,8),
                new SQLiteParameter("@PlanLeft5", DbType.Int32,8),
                new SQLiteParameter("@PlanRight1", DbType.Int32,8),
                new SQLiteParameter("@PlanRight2", DbType.Int32,8),
                new SQLiteParameter("@PlanRight3", DbType.Int32,8),
                new SQLiteParameter("@PlanRight4", DbType.Int32,8),
                new SQLiteParameter("@PlanRight5", DbType.Int32,8),
                new SQLiteParameter("@LightClass", DbType.Int32,8),
                new SQLiteParameter("@DisBoxID", DbType.Int32,8),
                new SQLiteParameter("@LightIndex", DbType.Int32,8),
                new SQLiteParameter("@IsEmergency", DbType.Int32,8),
                new SQLiteParameter("@RtnDirection",DbType.Decimal,4),
                new SQLiteParameter("@EscapeLineID",DbType.Int32,8),
                new SQLiteParameter("@Shield",DbType.Int32,8)};
            parameters[0].Value = infoLight.Code;
            parameters[1].Value = infoLight.Address;
            parameters[2].Value = infoLight.Status;
            parameters[3].Value = infoLight.BeginStatus;
            parameters[4].Value = infoLight.CurrentState;
            parameters[5].Value = infoLight.ErrorTime;
            parameters[6].Value = infoLight.Disable;
            parameters[7].Value = infoLight.PlanLeft1;
            parameters[8].Value = infoLight.PlanLeft2;
            parameters[9].Value = infoLight.PlanLeft3;
            parameters[10].Value = infoLight.PlanLeft4;
            parameters[11].Value = infoLight.PlanLeft5;
            parameters[12].Value = infoLight.PlanRight1;
            parameters[13].Value = infoLight.PlanRight2;
            parameters[14].Value = infoLight.PlanRight3;
            parameters[15].Value = infoLight.PlanRight4;
            parameters[16].Value = infoLight.PlanRight5;
            parameters[17].Value = infoLight.LightClass;
            parameters[18].Value = infoLight.DisBoxID;
            parameters[19].Value = infoLight.LightIndex;
            parameters[20].Value = infoLight.IsEmergency;
            parameters[21].Value = infoLight.RtnDirection;
            parameters[22].Value = infoLight.EscapeLineID == null ? -1 : infoLight.EscapeLineID;
            parameters[23].Value = infoLight.Shield;

            object obj = DBHelperSQLite.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                infoLight.ID = Convert.ToInt32(obj);
                return infoLight.ID;
            }
        }
        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(LightInfo infoLight)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update Light set ");
            strSql.Append("Code=@Code,");
            strSql.Append("Address=@Address,");
            strSql.Append("Status=@Status,");
            strSql.Append("BeginStatus=@BeginStatus,");
            strSql.Append("CurrentState=@CurrentState,");
            strSql.Append("ErrorTime=@ErrorTime,");
            strSql.Append("Disable=@Disable,");
            strSql.Append("PlanLeft1=@PlanLeft1,");
            strSql.Append("PlanLeft2=@PlanLeft2,");
            strSql.Append("PlanLeft3=@PlanLeft3,");
            strSql.Append("PlanLeft4=@PlanLeft4,");
            strSql.Append("PlanLeft5=@PlanLeft5,");
            strSql.Append("PlanRight1=@PlanRight1,");
            strSql.Append("PlanRight2=@PlanRight2,");
            strSql.Append("PlanRight3=@PlanRight3,");
            strSql.Append("PlanRight4=@PlanRight4,");
            strSql.Append("PlanRight5=@PlanRight5,");
            strSql.Append("LightClass=@LightClass,");
            strSql.Append("DisBoxID=@DisBoxID,");
            strSql.Append("LightIndex=@LightIndex,");
            strSql.Append("IsEmergency=@IsEmergency,");
            strSql.Append("RtnDirection=@RtnDirection,");
            strSql.Append("EscapeLineID=@EscapeLineID,");
            strSql.Append("Shield=@Shield");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@Code", DbType.String),
                new SQLiteParameter("@Address", DbType.String),
                new SQLiteParameter("@Status", DbType.Int32,8),
                new SQLiteParameter("@BeginStatus", DbType.Int32,8),
                new SQLiteParameter("@CurrentState",DbType.Int32,8),
                new SQLiteParameter("@ErrorTime", DbType.String),
                new SQLiteParameter("@Disable", DbType.Int32,8),
                new SQLiteParameter("@PlanLeft1", DbType.Int32,8),
                new SQLiteParameter("@PlanLeft2", DbType.Int32,8),
                new SQLiteParameter("@PlanLeft3", DbType.Int32,8),
                new SQLiteParameter("@PlanLeft4", DbType.Int32,8),
                new SQLiteParameter("@PlanLeft5", DbType.Int32,8),
                new SQLiteParameter("@PlanRight1", DbType.Int32,8),
                new SQLiteParameter("@PlanRight2", DbType.Int32,8),
                new SQLiteParameter("@PlanRight3", DbType.Int32,8),
                new SQLiteParameter("@PlanRight4", DbType.Int32,8),
                new SQLiteParameter("@PlanRight5", DbType.Int32,8),
                new SQLiteParameter("@LightClass", DbType.Int32,8),
                new SQLiteParameter("@DisBoxID", DbType.Int32,8),
                new SQLiteParameter("@LightIndex", DbType.Int32,8),
                new SQLiteParameter("@IsEmergency", DbType.Int32,8),
                new SQLiteParameter("@RtnDirection",DbType.Decimal,4),
                new SQLiteParameter("@EscapeLineID",DbType.Int32,8),
                new SQLiteParameter("@Shield",DbType.Int32,8),
                new SQLiteParameter("@ID", DbType.Int32,8)};
            parameters[0].Value = infoLight.Code;
            parameters[1].Value = infoLight.Address;
            parameters[2].Value = infoLight.Status;
            parameters[3].Value = infoLight.BeginStatus;
            parameters[4].Value = infoLight.CurrentState;
            parameters[5].Value = infoLight.ErrorTime;
            parameters[6].Value = infoLight.Disable;
            parameters[7].Value = infoLight.PlanLeft1;
            parameters[8].Value = infoLight.PlanLeft2;
            parameters[9].Value = infoLight.PlanLeft3;
            parameters[10].Value = infoLight.PlanLeft4;
            parameters[11].Value = infoLight.PlanLeft5;
            parameters[12].Value = infoLight.PlanRight1;
            parameters[13].Value = infoLight.PlanRight2;
            parameters[14].Value = infoLight.PlanRight3;
            parameters[15].Value = infoLight.PlanRight4;
            parameters[16].Value = infoLight.PlanRight5;
            parameters[17].Value = infoLight.LightClass;
            parameters[18].Value = infoLight.DisBoxID;
            parameters[19].Value = infoLight.LightIndex;
            parameters[20].Value = infoLight.IsEmergency;
            parameters[21].Value = infoLight.RtnDirection;
            parameters[22].Value = infoLight.EscapeLineID == null ? -1 : infoLight.EscapeLineID;
            parameters[23].Value = infoLight.Shield == null ? 0 : infoLight.Shield;
            parameters[24].Value = infoLight.ID;
            int rows = 0;
            rows = DBHelperSQLite.ExecuteSql(strSql.ToString(), parameters);
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
            string strSql = "delete from Light;";
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
        /// 保存多条数据
        /// </summary>
        /// <returns></returns>
        public int Save(List<LightInfo> LstLight)
        {
            List<String> SQLStringList = new List<string>();
            foreach (LightInfo infoLight in LstLight)
            {
                StringBuilder strSql = new StringBuilder();
                if (infoLight.ID == 0)
                {
                    strSql.Append(string.Format("insert into Light values("));
                    strSql.Append(string.Format("(select (max(ID) + 1) from Light),"));
                    strSql.Append(string.Format("'{0}','{1}',{2},{3},{4},'{5}',{6},{7},'{8}',{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23});", infoLight.Code, infoLight.Address, infoLight.Status, infoLight.BeginStatus, infoLight.CurrentState, infoLight.ErrorTime, infoLight.Disable,
                        infoLight.PlanLeft1, infoLight.PlanLeft2, infoLight.PlanLeft3, infoLight.PlanLeft4, infoLight.PlanLeft5
                        , infoLight.PlanRight1, infoLight.PlanRight2, infoLight.PlanRight3, infoLight.PlanRight4
                        , infoLight.PlanRight5, infoLight.LightClass, infoLight.DisBoxID, infoLight.LightIndex, infoLight.IsEmergency, infoLight.RtnDirection, infoLight.EscapeLineID, infoLight.Shield));
                }
                else
                {
                    strSql.Append(string.Format("update Light set "));
                    strSql.Append(string.Format("Code='{0}',", infoLight.Code));
                    strSql.Append(string.Format("Address='{0}',", infoLight.Address));
                    strSql.Append(string.Format("Status={0},", infoLight.Status));
                    strSql.Append(string.Format("BeginStatus={0},", infoLight.BeginStatus));
                    strSql.Append(string.Format("CurrentState={0},", infoLight.CurrentState));
                    strSql.Append(string.Format("ErrorTime='{0}',", infoLight.ErrorTime));
                    strSql.Append(string.Format("Disable={0},", infoLight.Disable));
                    strSql.Append(string.Format("PlanLeft1={0},", infoLight.PlanLeft1));
                    strSql.Append(string.Format("PlanLeft2={0},", infoLight.PlanLeft2));
                    strSql.Append(string.Format("PlanLeft3={0},", infoLight.PlanLeft3));
                    strSql.Append(string.Format("PlanLeft4={0},", infoLight.PlanLeft4));
                    strSql.Append(string.Format("PlanLeft5={0},", infoLight.PlanLeft5));
                    strSql.Append(string.Format("PlanRight1={0},", infoLight.PlanRight1));
                    strSql.Append(string.Format("PlanRight2={0},", infoLight.PlanRight2));
                    strSql.Append(string.Format("PlanRight3={0},", infoLight.PlanRight3));
                    strSql.Append(string.Format("PlanRight4={0},", infoLight.PlanRight4));
                    strSql.Append(string.Format("PlanRight5={0},", infoLight.PlanRight5));
                    strSql.Append(string.Format("LightClass={0},", infoLight.LightClass));
                    strSql.Append(string.Format("DisBoxID={0},", infoLight.DisBoxID));
                    strSql.Append(string.Format("LightIndex={0},", infoLight.LightIndex));
                    strSql.Append(string.Format("IsEmergency={0},", infoLight.IsEmergency));
                    strSql.Append(string.Format("RtnDirection={0},", infoLight.RtnDirection));
                    strSql.Append(string.Format("EscapeLineID={0},", infoLight.EscapeLineID == null ? -1 : infoLight.EscapeLineID));
                    strSql.Append(string.Format("Shield={0}", infoLight.Shield == null ? 0 : infoLight.Shield));
                    strSql.Append(string.Format(" where ID={0};", infoLight.ID));
                }
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<LightInfo> LstLight)
        {
            List<String> SQLStringList = new List<string>();
            foreach (LightInfo infoLight in LstLight)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(string.Format("insert into Light values("));
                strSql.Append(string.Format("{0},'{1}','{2}',{3},{4},{5},'{6}',{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24});", infoLight.ID, infoLight.Code, infoLight.Address, infoLight.Status, infoLight.BeginStatus, infoLight.CurrentState, infoLight.ErrorTime, infoLight.Disable,
                    infoLight.PlanLeft1, infoLight.PlanLeft2, infoLight.PlanLeft3, infoLight.PlanLeft4, infoLight.PlanLeft5,
                    infoLight.PlanRight1, infoLight.PlanRight2, infoLight.PlanRight3, infoLight.PlanRight4, infoLight.PlanRight5, infoLight.LightClass, infoLight.DisBoxID, infoLight.LightIndex, infoLight.IsEmergency, infoLight.RtnDirection, infoLight.EscapeLineID == null ? -1 : infoLight.EscapeLineID, infoLight.Shield == null ? 0 : infoLight.Shield));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        public bool Delete(int ID)
        {
            string strSql = string.Format("delete from Light where ID = {0}", ID);
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
        public List<LightInfo> GetAll()
        {
            string strSql = "select * From Light;";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<LightInfo>>(strJsonString);
        }

        #endregion  BasicMethod
    }
}

