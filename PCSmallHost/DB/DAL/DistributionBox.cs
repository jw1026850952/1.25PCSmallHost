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
    /// 数据访问类:DistributionBox
    /// </summary>
    public partial class DistributionBox
    {
        public DistributionBox()
        { }
        #region  BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        public int Add(DistributionBoxInfo infoDistributionBox)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into DistributionBox(");
            strSql.Append("Code,Address,Status,MainEleVoltage,DischargeVoltage,BatteryVoltage,DischargeCurrent,ErrorTime,Disable,Plan1,Plan2,Plan3,Plan4,Plan5,IsEmergency,Test,QiangQi,AutoManual,Shield)");
            strSql.Append(" values (");
            strSql.Append("@Code,@Address,@Status,@MainEleVoltage,@DischargeVoltage,@BatteryVoltage,@DischargeCurrent,@ErrorTime,@Disable,@Plan1,@Plan2,@Plan3,@Plan4,@Plan5,@IsEmergency,@Test,@QiangQi,@AutoManual,@Shield)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters = {
                    new SQLiteParameter("@Code", DbType.String),
                    new SQLiteParameter("@Address", DbType.String),
                    new SQLiteParameter("@Status", DbType.Int32,8),
                    new SQLiteParameter("@MainEleVoltage", DbType.Decimal,4),
                    new SQLiteParameter("@DischargeVoltage", DbType.Decimal,4),
                    new SQLiteParameter("@BatteryVoltage", DbType.Decimal,4),
                    new SQLiteParameter("@DischargeCurrent", DbType.Decimal,4),
                    new SQLiteParameter("@ErrorTime", DbType.String),
                    new SQLiteParameter("@Disable", DbType.Int32,8),
                    new SQLiteParameter("@Plan1", DbType.Int32,8),
                    new SQLiteParameter("@Plan2", DbType.Int32,8),
                    new SQLiteParameter("@Plan3", DbType.Int32,8),
                    new SQLiteParameter("@Plan4", DbType.Int32,8),
                    new SQLiteParameter("@Plan5", DbType.Int32,8),
                    new SQLiteParameter("@IsEmergency", DbType.Int32,4),
                    new SQLiteParameter("@Test",DbType.Int32,4),
                    new SQLiteParameter("@QiangQi",DbType.Int32,4),
                    new SQLiteParameter("@AutoManual",DbType.Int32,4),
                    new SQLiteParameter("@Shield",DbType.Int32,8)};
            parameters[0].Value = infoDistributionBox.Code;
            parameters[1].Value = infoDistributionBox.Address;
            parameters[2].Value = infoDistributionBox.Status;
            parameters[3].Value = infoDistributionBox.MainEleVoltage;
            parameters[4].Value = infoDistributionBox.DischargeVoltage;
            parameters[5].Value = infoDistributionBox.BatteryVoltage;
            parameters[6].Value = infoDistributionBox.DischargeCurrent;
            parameters[7].Value = infoDistributionBox.ErrorTime;
            parameters[8].Value = infoDistributionBox.Disable;
            parameters[9].Value = infoDistributionBox.Plan1;
            parameters[10].Value = infoDistributionBox.Plan2;
            parameters[11].Value = infoDistributionBox.Plan3;
            parameters[12].Value = infoDistributionBox.Plan4;
            parameters[13].Value = infoDistributionBox.Plan5;
            parameters[14].Value = infoDistributionBox.IsEmergency;
            parameters[15].Value = infoDistributionBox.Test;
            parameters[16].Value = infoDistributionBox.QiangQi;
            parameters[17].Value = infoDistributionBox.AutoManual;
            parameters[18].Value = infoDistributionBox.Shield == null ? 0 : infoDistributionBox.Shield;

            object obj = DBHelperSQLite.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                infoDistributionBox.ID = Convert.ToInt32(obj);
                return infoDistributionBox.ID;
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        public bool Update(DistributionBoxInfo infoDistributionBox)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update DistributionBox set ");
            strSql.Append("Code=@Code,");
            strSql.Append("Address=@Address,");
            strSql.Append("Status=@Status,");
            strSql.Append("MainEleVoltage=@MainEleVoltage,");
            strSql.Append("DischargeVoltage=@DischargeVoltage,");
            strSql.Append("BatteryVoltage=@BatteryVoltage,");
            strSql.Append("DischargeCurrent=@DischargeCurrent,");
            strSql.Append("ErrorTime=@ErrorTime,");
            strSql.Append("Disable=@Disable,");
            strSql.Append("Plan1=@Plan1,");
            strSql.Append("Plan2=@Plan2,");
            strSql.Append("Plan3=@Plan3,");
            strSql.Append("Plan4=@Plan4,");
            strSql.Append("Plan5=@Plan5,");
            strSql.Append("IsEmergency=@IsEmergency,");
            strSql.Append("Test=@Test,");
            strSql.Append("QiangQi=@QiangQi,");
            strSql.Append("AutoManual=@AutoManual,");
            strSql.Append("Shield=@Shield");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters = {
                new SQLiteParameter("@Code", DbType.String),
                new SQLiteParameter("@Address", DbType.String),
                new SQLiteParameter("@Status", DbType.Int32,8),
                new SQLiteParameter("@MainEleVoltage", DbType.Decimal,4),
                new SQLiteParameter("@DischargeVoltage", DbType.Decimal,4),
                new SQLiteParameter("@BatteryVoltage", DbType.Decimal,4),
                new SQLiteParameter("@DischargeCurrent", DbType.Decimal,4),
                new SQLiteParameter("@ErrorTime", DbType.String),
                new SQLiteParameter("@Disable", DbType.Int32,8),
                new SQLiteParameter("@Plan1", DbType.Int32,8),
                new SQLiteParameter("@Plan2", DbType.Int32,8),
                new SQLiteParameter("@Plan3", DbType.Int32,8),
                new SQLiteParameter("@Plan4", DbType.Int32,8),
                new SQLiteParameter("@Plan5", DbType.Int32,8),
                new SQLiteParameter("@IsEmergency", DbType.Int32, 4),
                new SQLiteParameter("@Test",DbType.Int32,4),
                new SQLiteParameter("@QiangQi",DbType.Int32,4),
                new SQLiteParameter("@AutoManual",DbType.Int32,4),
                new SQLiteParameter("@Shield",DbType.Int32,8),
                new SQLiteParameter("@ID", DbType.Int32,8)};
            parameters[0].Value = infoDistributionBox.Code;
            parameters[1].Value = infoDistributionBox.Address;
            parameters[2].Value = infoDistributionBox.Status;
            parameters[3].Value = infoDistributionBox.MainEleVoltage;
            parameters[4].Value = infoDistributionBox.DischargeVoltage;
            parameters[5].Value = infoDistributionBox.BatteryVoltage;
            parameters[6].Value = infoDistributionBox.DischargeCurrent;
            parameters[7].Value = infoDistributionBox.ErrorTime;
            parameters[8].Value = infoDistributionBox.Disable;
            parameters[9].Value = infoDistributionBox.Plan1;
            parameters[10].Value = infoDistributionBox.Plan2;
            parameters[11].Value = infoDistributionBox.Plan3;
            parameters[12].Value = infoDistributionBox.Plan4;
            parameters[13].Value = infoDistributionBox.Plan5;
            parameters[14].Value = infoDistributionBox.IsEmergency;
            parameters[15].Value = infoDistributionBox.Test;
            parameters[16].Value = infoDistributionBox.QiangQi;
            parameters[17].Value = infoDistributionBox.AutoManual;
            parameters[18].Value = infoDistributionBox.Shield == null ? 0 : infoDistributionBox.Shield;
            parameters[19].Value = infoDistributionBox.ID;

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
        public int RestoreOrBackUpData(List<DistributionBoxInfo> LstDistributionBox)
        {
            List<String> SQLStringList = new List<string>();
            foreach (DistributionBoxInfo infoDistributionBox in LstDistributionBox)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(string.Format("insert into DistributionBox values("));
                strSql.Append(string.Format("{0},'{1}','{2}',{3},{4},{5},{6},{7},'{8}',{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19});", infoDistributionBox.ID, infoDistributionBox.Code, infoDistributionBox.Address, infoDistributionBox.Status, infoDistributionBox.MainEleVoltage, infoDistributionBox.DischargeVoltage, infoDistributionBox.BatteryVoltage, infoDistributionBox.DischargeCurrent, infoDistributionBox.ErrorTime, infoDistributionBox.Disable, infoDistributionBox.Plan1, infoDistributionBox.Plan2, infoDistributionBox.Plan3, infoDistributionBox.Plan4, infoDistributionBox.Plan5, infoDistributionBox.IsEmergency, infoDistributionBox.Test, infoDistributionBox.QiangQi, infoDistributionBox.AutoManual, infoDistributionBox.Shield == null ? 0 : infoDistributionBox.Shield));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        public int Save(List<DistributionBoxInfo> LstDistributionBox)
        {
            List<String> SQLStringList = new List<string>();
            foreach (DistributionBoxInfo infoDistributionBox in LstDistributionBox)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update DistributionBox set ");
                strSql.Append(string.Format("Code='{0}',", infoDistributionBox.Code));
                strSql.Append(string.Format("Address='{0}',", infoDistributionBox.Address));
                strSql.Append(string.Format("Status={0},", infoDistributionBox.Status));
                strSql.Append(string.Format("MainEleVoltage={0},", infoDistributionBox.MainEleVoltage));
                strSql.Append(string.Format("DischargeVoltage={0},", infoDistributionBox.DischargeVoltage));
                strSql.Append(string.Format("BatteryVoltage={0},", infoDistributionBox.BatteryVoltage));
                strSql.Append(string.Format("DischargeCurrent={0},", infoDistributionBox.DischargeCurrent));
                strSql.Append(string.Format("ErrorTime='{0}',", infoDistributionBox.ErrorTime));
                strSql.Append(string.Format("Disable={0},", infoDistributionBox.Disable));
                strSql.Append(string.Format("Plan1={0},", infoDistributionBox.Plan1));
                strSql.Append(string.Format("Plan2={0},", infoDistributionBox.Plan2));
                strSql.Append(string.Format("Plan3={0},", infoDistributionBox.Plan3));
                strSql.Append(string.Format("Plan4={0},", infoDistributionBox.Plan4));
                strSql.Append(string.Format("Plan5={0},", infoDistributionBox.Plan5));
                strSql.Append(string.Format("IsEmergency={0},", infoDistributionBox.IsEmergency));
                strSql.Append(string.Format("Test={0},", infoDistributionBox.Test));
                strSql.Append(string.Format("QiangQi={0},", infoDistributionBox.QiangQi));
                strSql.Append(string.Format("AutoManual={0},", infoDistributionBox.AutoManual));
                strSql.Append(string.Format("Shield={0}", infoDistributionBox.Shield == null ? 0 : infoDistributionBox.Shield));
                strSql.Append(string.Format(" where ID={0};", infoDistributionBox.ID));
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
            string strSql = "delete from DistributionBox;";
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
        public bool Delete(int ID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("delete from DistributionBox ");
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

        /// <summary>
        /// 获得数据列表
        /// </summary>
        public List<DistributionBoxInfo> GetAll()
        {
            string strSql = "select * from DistributionBox;";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<DistributionBoxInfo>>(strJsonString);
        }

        #endregion  BasicMethod
    }
}

