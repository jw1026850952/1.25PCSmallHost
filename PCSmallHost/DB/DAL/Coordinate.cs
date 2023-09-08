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
    public partial class Coordinate
    {
        public Coordinate() { }

        #region BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="infoCoordinate"></param>
        /// <returns></returns>
        public int Add(CoordinateInfo infoCoordinate)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into Coordinate(");
            strSql.Append("TableName,TableID,Location,OriginX,OriginY,NLOriginX,NLOriginY,TransformX,TransformY,IsAuth)");
            strSql.Append(" values (");
            strSql.Append("@TableName,@TableID,@Location,@OriginX,@OriginY,@NLOriginX,@NLOriginY,@TransformX,@TransformY,@IsAuth)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters =
            {
                new SQLiteParameter("@TableName",DbType.String),
                new SQLiteParameter("@TableID",DbType.Int32,4),
                new SQLiteParameter("@Location",DbType.Int32,4),
                new SQLiteParameter("@OriginX",DbType.Decimal,4),
                new SQLiteParameter("@OriginY",DbType.Decimal,4),
                new SQLiteParameter("@NLOriginX",DbType.Decimal,4),
                new SQLiteParameter("@NLOriginY",DbType.Decimal,4),
                new SQLiteParameter("@TransformX",DbType.Decimal,4),
                new SQLiteParameter("@TransformY",DbType.Decimal,4),
                new SQLiteParameter("@IsAuth",DbType.Int32,4)
            };
            parameters[0].Value = infoCoordinate.TableName;
            parameters[1].Value = infoCoordinate.TableID;
            parameters[2].Value = infoCoordinate.Location;
            parameters[3].Value = infoCoordinate.OriginX;
            parameters[4].Value = infoCoordinate.OriginY;
            parameters[5].Value = infoCoordinate.NLOriginX;
            parameters[6].Value = infoCoordinate.NLOriginY;
            parameters[7].Value = infoCoordinate.TransformX;
            parameters[8].Value = infoCoordinate.TransformY;
            parameters[9].Value = infoCoordinate.IsAuth;

            object obj = DBHelperSQLite.GetSingle(strSql.ToString(), parameters);
            if(obj == null)
            {
                return 0;
            }
            else
            {
                infoCoordinate.ID = Convert.ToInt32(obj);
                return infoCoordinate.ID;
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoCoordinate"></param>
        /// <returns></returns>
        public bool Update(CoordinateInfo infoCoordinate)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update Coordinate set ");
            strSql.Append("TableName=@TableName,");
            strSql.Append("TableID=@TableID,");
            strSql.Append("Location=@Location,");
            strSql.Append("OriginX=@OriginX,");
            strSql.Append("OriginY=@OriginY,");
            strSql.Append("NLOriginX=@NLOriginX,");
            strSql.Append("NLOriginY=@NLOriginY,");
            strSql.Append("TransformX=@TransformX,");
            strSql.Append("TransformY=@TransformY,");
            strSql.Append("IsAuth=@IsAuth");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters =
            {
                new SQLiteParameter("@TableName",DbType.String),
                new SQLiteParameter("@TableID",DbType.Int32,4),
                new SQLiteParameter("@Location",DbType.Int32,4),
                new SQLiteParameter("@OriginX",DbType.Decimal,4),
                new SQLiteParameter("@OriginY",DbType.Decimal,4),
                new SQLiteParameter("@NLOriginX",DbType.Decimal,4),
                new SQLiteParameter("@NLOriginY",DbType.Decimal,4),
                new SQLiteParameter("@TransformX",DbType.Decimal,4),
                new SQLiteParameter("@TransformY",DbType.Decimal,4),
                new SQLiteParameter("@IsAuth",DbType.Int32,4),
                new SQLiteParameter("@ID",DbType.Int32,4)
            };
            parameters[0].Value = infoCoordinate.TableName;
            parameters[1].Value = infoCoordinate.TableID;
            parameters[2].Value = infoCoordinate.Location;
            parameters[3].Value = infoCoordinate.OriginX;
            parameters[4].Value = infoCoordinate.OriginY;
            parameters[5].Value = infoCoordinate.NLOriginX;
            parameters[6].Value = infoCoordinate.NLOriginY;
            parameters[7].Value = infoCoordinate.TransformX;
            parameters[8].Value = infoCoordinate.TransformY;
            parameters[9].Value = infoCoordinate.IsAuth;
            parameters[10].Value = infoCoordinate.ID;

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
        /// 备份数据或者恢复备份数据
        /// </summary>
        /// <param name="LstCoordinate"></param>
        /// <returns></returns>
        public int RestoreOrBackUpData(List<CoordinateInfo> LstCoordinate)
        {
            List<string> SQLStringList = new List<string>();
            foreach (CoordinateInfo infoCoordinate in LstCoordinate)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(string.Format("insert into Coordinate values("));
                strSql.Append(string.Format("{0},'{1}',{2},{3},{4},{5},{6},{7},{8},{9},{10});", infoCoordinate.ID, infoCoordinate.TableName, infoCoordinate.TableID, infoCoordinate.Location, infoCoordinate.OriginX, infoCoordinate.OriginY, infoCoordinate.NLOriginX, infoCoordinate.NLOriginY, infoCoordinate.TransformX, infoCoordinate.TransformY,infoCoordinate.IsAuth));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstCoordinate"></param>
        /// <returns></returns>
        public int Save(List<CoordinateInfo> LstCoordinate)
        {
            List<string> SQLStringList = new List<string>();
            foreach(CoordinateInfo infoCoordinate in LstCoordinate)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update Coordinate set ");
                strSql.Append(string.Format("TableName='{0}',", infoCoordinate.TableName));
                strSql.Append(string.Format("TableID={0},", infoCoordinate.TableID));
                strSql.Append(string.Format("Location={0},", infoCoordinate.Location));
                strSql.Append(string.Format("OriginX={0},", infoCoordinate.OriginX));
                strSql.Append(string.Format("OriginY={0},", infoCoordinate.OriginY));
                strSql.Append(string.Format("NLOriginX={0},", infoCoordinate.NLOriginX));
                strSql.Append(string.Format("NLOriginY={0},", infoCoordinate.NLOriginY));
                strSql.Append(string.Format("TransformX={0},", infoCoordinate.TransformX));
                strSql.Append(string.Format("TransformY={0},", infoCoordinate.TransformY));
                strSql.Append(string.Format("IsAuth={0}", infoCoordinate.IsAuth));
                strSql.Append(string.Format(" where ID={0};", infoCoordinate.ID));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }

        public bool DeleteAll()
        {
            string strSql = "delete from Coordinate;";
            int rows=DBHelperSQLite.ExecuteSql(strSql);
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
            strSql.Append("delete from Coordinate ");
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
        public List<CoordinateInfo> GetAll()
        {
            string strSql = "select * from Coordinate;";
            DataSet ds =DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<CoordinateInfo>>(strJsonString);
        }
        #endregion
    }
}
