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
    public class EscapeLines
    {
        public EscapeLines()
        { }

        #region BasicMethod

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="infoEscapeLines"></param>
        /// <returns></returns>
        public int Add(EscapeLinesInfo infoEscapeLines)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("insert into EscapeLines(");
            strSql.Append("Name,Location,LineX1,LineY1,LineX2,LineY2,TransformX1,TransformY1,TransformX2,TransformY2,NLineX1,NLineY1,NLineX2,NLineY2)");
            strSql.Append("values(");
            strSql.Append("@Name,@Location,@LineX1,@LineY1,@LineX2,@LineY2,@TransformX1,@TransformY1,@TransformX2,@TransformY2,@NLineX1,@NLineY1,@NLineX2,@NLineY2)");
            strSql.Append(";select LAST_INSERT_ROWID()");
            SQLiteParameter[] parameters ={
                    new SQLiteParameter("@Name",DbType.String),
                    new SQLiteParameter("@Location",DbType.Int32,8),
                    new SQLiteParameter("@LineX1",DbType.Decimal,4),
                    new SQLiteParameter("@LineY1",DbType.Decimal,4),
                    new SQLiteParameter("@LineX2",DbType.Decimal,4),
                    new SQLiteParameter("@LineY2",DbType.Decimal,4),
                    new SQLiteParameter("@TransformX1",DbType.Decimal,4),
                    new SQLiteParameter("@TransformY1",DbType.Decimal,4),
                    new SQLiteParameter("@TransformX2",DbType.Decimal,4),
                    new SQLiteParameter("@TransformY2",DbType.Decimal,4),
                    new SQLiteParameter("@NLineX1",DbType.Decimal,4),
                    new SQLiteParameter("@NLineY1",DbType.Decimal,4),
                    new SQLiteParameter("@NLineX2",DbType.Decimal,4),
                    new SQLiteParameter("@NLineY2",DbType.Decimal,4)};
            parameters[0].Value = infoEscapeLines.Name;
            parameters[1].Value = infoEscapeLines.Location;
            parameters[2].Value = infoEscapeLines.LineX1;
            parameters[3].Value = infoEscapeLines.LineY1;
            parameters[4].Value = infoEscapeLines.LineX2;
            parameters[5].Value = infoEscapeLines.LineY2;
            parameters[6].Value = infoEscapeLines.TransformX1;
            parameters[7].Value = infoEscapeLines.TransformY1;
            parameters[8].Value = infoEscapeLines.TransformX2;
            parameters[9].Value = infoEscapeLines.TransformY2;
            parameters[10].Value = infoEscapeLines.NLineX1;
            parameters[11].Value = infoEscapeLines.NLineY1;
            parameters[12].Value = infoEscapeLines.NLineX2;
            parameters[13].Value = infoEscapeLines.NLineY2;

            object obj = DBHelperSQLite.GetSingle(strSql.ToString(), parameters);
            if (obj == null)
            {
                return 0;
            }
            else
            {
                infoEscapeLines.ID = Convert.ToInt32(obj);
                return infoEscapeLines.ID;
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="infoEscapeRoutes"></param>
        /// <returns></returns>
        public bool Update(EscapeLinesInfo infoEscapeLines)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("update EscapeLines set ");
            strSql.Append("Name=@Name,");
            strSql.Append("Location=@Location,");
            strSql.Append("LineX1=@LineX1,");
            strSql.Append("LineY1=@LineY1,");
            strSql.Append("LineX2=@LineX2,");
            strSql.Append("LineY2=@LineY2,");
            strSql.Append("TransformX1=@TransformX1,");
            strSql.Append("TransformY1=@TransformY1,");
            strSql.Append("TransformX2=@TransformX2,");
            strSql.Append("TransformY2=@TransformY2,");
            strSql.Append("NLineX1=@NLineX1,");
            strSql.Append("NLineY1=@NLineY1,");
            strSql.Append("NLineX2=@NLineX2,");
            strSql.Append("NLineY2=@NLineY2");
            strSql.Append(" where ID=@ID");
            SQLiteParameter[] parameters ={
                    new SQLiteParameter("@Name",DbType.String),
                    new SQLiteParameter("@Location",DbType.Int32,8),
                    new SQLiteParameter("@LineX1",DbType.Decimal,4),
                    new SQLiteParameter("@LineY1",DbType.Decimal,4),
                    new SQLiteParameter("@LineX2",DbType.Decimal,4),
                    new SQLiteParameter("@LineY2",DbType.Decimal,4),
                    new SQLiteParameter("@TransformX1",DbType.Decimal,4),
                    new SQLiteParameter("@TransformY1",DbType.Decimal,4),
                    new SQLiteParameter("@TransformX2",DbType.Decimal,4),
                    new SQLiteParameter("@TransformY2",DbType.Decimal,4),
                    new SQLiteParameter("@NLineX1",DbType.Decimal,4),
                    new SQLiteParameter("@NLineY1",DbType.Decimal,4),
                    new SQLiteParameter("@NLineX2",DbType.Decimal,4),
                    new SQLiteParameter("@NLineY2",DbType.Decimal,4),
                    new SQLiteParameter("@ID",DbType.Int32,8)};
            parameters[0].Value = infoEscapeLines.Name;
            parameters[1].Value = infoEscapeLines.Location;
            parameters[2].Value = infoEscapeLines.LineX1;
            parameters[3].Value = infoEscapeLines.LineY1;
            parameters[4].Value = infoEscapeLines.LineX2;
            parameters[5].Value = infoEscapeLines.LineY2;
            parameters[6].Value = infoEscapeLines.TransformX1;
            parameters[7].Value = infoEscapeLines.TransformY1;
            parameters[8].Value = infoEscapeLines.TransformX2;
            parameters[9].Value = infoEscapeLines.TransformY2;
            parameters[10].Value = infoEscapeLines.NLineX1;
            parameters[11].Value = infoEscapeLines.NLineY1;
            parameters[12].Value = infoEscapeLines.NLineX2;
            parameters[13].Value = infoEscapeLines.NLineY2;
            parameters[14].Value = infoEscapeLines.ID;

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
            strSql.Append("delete from EscapeLines ");
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
            string strSql = "delete from EscapeLines;";
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
        public List<EscapeLinesInfo> GetAll()
        {
            string strSql = "select * From EscapeLines Order By Location";
            DataSet ds = DBHelperSQLite.Query(strSql);
            string strJsonString = JsonConvert.SerializeObject(ds.Tables[0]);
            return JsonConvert.DeserializeObject<List<EscapeLinesInfo>>(strJsonString);
        }

        /// <summary>
        /// 保存多条数据
        /// </summary>
        /// <param name="LstEscapeRoutes"></param>
        /// <returns></returns>
        public int Save(List<EscapeLinesInfo> LstEscapeLines)
        {
            List<String> SQLStringList = new List<string>();
            foreach (EscapeLinesInfo infoEscapeLines in LstEscapeLines)
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("update EscapeLines set ");
                strSql.Append(string.Format("ID={0},", infoEscapeLines.ID));
                strSql.Append(string.Format("Name='{0}',", infoEscapeLines.Name));
                strSql.Append(string.Format("Location={0},", infoEscapeLines.Location));
                strSql.Append(string.Format("LineX1={0},", infoEscapeLines.LineX1));
                strSql.Append(string.Format("LineY1={0},", infoEscapeLines.LineY1));
                strSql.Append(string.Format("LineX2={0},", infoEscapeLines.LineX2));
                strSql.Append(string.Format("LineY2={0},", infoEscapeLines.LineY2));
                strSql.Append(string.Format("TransformX1={0},", infoEscapeLines.TransformX1));
                strSql.Append(string.Format("TransformY1={0},", infoEscapeLines.TransformY1));
                strSql.Append(string.Format("TransformX2={0},", infoEscapeLines.TransformX2));
                strSql.Append(string.Format("TransformY2={0},", infoEscapeLines.TransformY2));
                strSql.Append(string.Format("NLineX1={0},", infoEscapeLines.NLineX1));
                strSql.Append(string.Format("NLineY1={0},", infoEscapeLines.NLineY1));
                strSql.Append(string.Format("NLineX2={0},", infoEscapeLines.NLineX2));
                strSql.Append(string.Format("NLineY2={0}", infoEscapeLines.NLineY2));
                strSql.Append(string.Format(" where ID={0};", infoEscapeLines.ID));
                SQLStringList.Add(strSql.ToString());
            }
            return DBHelperSQLite.ExecuteSqlTran(SQLStringList);
        }
        #endregion
    }
}
