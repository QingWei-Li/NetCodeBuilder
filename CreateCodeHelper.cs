using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBuilder
{
    public class CreateCodeHelper
    {

        //进行可空类型处理
        private static string GetDataTypeName(DataColumn column)
        {
            //如果列允许为NULL，并且在.NET中的类型不能为空（值类型）
            if (column.AllowDBNull && column.DataType.IsValueType)
            {
                return column.DataType + "?";
            }
            else
            {
                return column.DataType.ToString();
            }
        }

        //进行可空类型处理，String为空要特殊处理
        private static string GetDataTypeNameString(DataColumn column)
        {
            //如果列允许为NULL，并且在.NET中的类型不能为空（值类型）
            if (column.AllowDBNull)
            {
                return column.DataType + "?";
            }
            else
            {
                return column.DataType.ToString();
            }
        }

        //以数组形式返回列名
        private static string[] GetColumnNames(DataTable dt)
        {
            string[] colNames = new string[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                DataColumn dataCol = dt.Columns[i];
                colNames[i] = dataCol.ColumnName;
            }
            return colNames;
        }

        //以数组形式返回"列名=@列名"
        private static string[] GetColumnNamesUpdate(DataTable dt)
        {
            string[] colNames = new string[dt.Columns.Count - 1];
            for (int i = 1; i < dt.Columns.Count; i++)
            {
                DataColumn dataCol = dt.Columns[i];
                colNames[i - 1] = dataCol.ColumnName + "=@" + dataCol.ColumnName;
            }
            return colNames;
        }

        //得到 new SqlParameter
        private static void GetSqlParameter(DataTable dt, StringBuilder sb, string hasNamespace)
        {
            foreach (DataColumn col in dt.Columns)
            {
                if (GetDataTypeNameString(col).IndexOf("?") > 0)
                {
                    sb.AppendLine(hasNamespace + "        ,new SqlParameter(\"@" + col.ColumnName + "\", SqlHelper.ToDBValue(model." + col.ColumnName + "))");
                }
                else
                {
                    sb.AppendLine(hasNamespace + "        ,new SqlParameter(\"@" + col.ColumnName + "\", model." + col.ColumnName + ")");
                }
            }
        }

        //生成Model带命名空间
        public StringBuilder CreateModelCode(string tableName, DataTable dt, string strNamespace)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("namespace " + strNamespace + " {");
            sb.AppendLine("    public class " + tableName + " {");
            foreach (DataColumn col in dt.Columns)
            {
                sb.AppendLine("        public " + GetDataTypeName(col) + " " + col.ColumnName + " {get; set;}");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb;
        }

        //生成Model不带命名空间
        public StringBuilder CreateModelCode(string tableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("public class " + tableName + " {");
            foreach (DataColumn col in dt.Columns)
            {
                sb.AppendLine("    public " + GetDataTypeName(col) + " " + col.ColumnName + " {get; set;}");
            }
            sb.AppendLine("}");
            return sb;
        }

        //生成DAL不带命名空间
        public StringBuilder CreateDALCode(string tableName, DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            //public class tableNameDAL{
            sb.AppendLine("public class  " + tableName + "DAL {");
            sb.AppendLine("");
            CreateToModel(tableName, dt, sb, "");
            CreateInsert(tableName, dt, sb, "");
            CreateDeleteById(tableName, dt, sb, "");
            CreateUpdate(tableName, dt, sb, "");
            CreateGetById(tableName, dt, sb, "");
            CreateListAll(tableName, dt, sb, "");
            CreateListByWhere(tableName,sb,"");
            sb.AppendLine("}");//tableNameDAL
            return sb;
        }

        //生成DAL带命名空间
        public StringBuilder CreateDALCode(string tableName, DataTable dt, string strNamespace)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using Helper;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Data;");
            sb.AppendLine("using System.Data.SqlClient;");
            sb.AppendLine("");
            sb.AppendLine("namespace " + strNamespace + " {");
            //public class tableNameDAL{
            sb.AppendLine("    public class  " + tableName + "DAL {");
            sb.AppendLine("");
            CreateToModel(tableName, dt, sb, "    ");
            CreateInsert(tableName, dt, sb, "    ");
            CreateDeleteById(tableName, dt, sb, "    ");
            CreateUpdate(tableName, dt, sb, "    ");
            CreateGetById(tableName, dt, sb, "    ");
            CreateListAll(tableName, dt, sb, "    ");
            CreateListByWhere(tableName, sb, "  ");
            sb.AppendLine("    }");//tableNameDAL
            sb.AppendLine("}");
            return sb;
        }

        //生成ToModel
        public void CreateToModel(string tableName, DataTable dt, StringBuilder sb, string hasNamespace)
        {
            //private tableName ToModel(DataRow row){
            sb.AppendLine(hasNamespace + "    private " + tableName + " ToModel(DataRow row) {");
            //tableName model = new tableName();
            sb.AppendLine(hasNamespace + "        " + tableName + " model = new " + tableName + "();");
            foreach (DataColumn col in dt.Columns)
            {
                if (GetDataTypeNameString(col).IndexOf("?") > 0)
                {
                    //model.Password = (string)SqlHelper.FromDbValue(row["Password"]);
                    sb.AppendLine(hasNamespace + "        model." + col.ColumnName + " = (" + GetDataTypeName(col) + ")SqlHelper.FromDBValue(row[\"" + col.ColumnName + "\"]);");
                }
                else
                {
                    //model.Password = (string)row["Password"];
                    sb.AppendLine(hasNamespace + "        model." + col.ColumnName + " = (" + GetDataTypeName(col) + ")row[\"" + col.ColumnName + "\"];");
                }
            }
            sb.AppendLine(hasNamespace + "        return model;");
            sb.AppendLine(hasNamespace + "    }");//ToModel
            sb.AppendLine("");
        }

        //生成ListAll
        public void CreateListAll(string tableName, DataTable dt, StringBuilder sb, string hasNamespace)
        {
            sb.AppendLine(hasNamespace + "    /// <summary>");
            sb.AppendLine(hasNamespace + "    /// 获得所有记录");
            sb.AppendLine(hasNamespace + "    /// </summary>");
            sb.AppendLine(hasNamespace + "    /// <returns>" + tableName + "类的对象的枚举</returns>");
            //    public IEnumerable<Role> ListAll() {
            sb.AppendLine(hasNamespace + "    public IEnumerable<" + tableName + "> ListAll() {");
            //    List<Role> list = new List<Role>();
            sb.AppendLine(hasNamespace + "        List<" + tableName + "> list = new List<" + tableName + ">();");
            //    DataTable dt = SqlHelper.ExecuteDataTable("SELECT * FROM Role");
            string[] colNames = GetColumnNames(dt);
            sb.AppendLine(hasNamespace + "        DataTable dt = SqlHelper.ExecuteDataTable(\"SELECT " + string.Join(", ", colNames) + " FROM " + tableName + "\");");
            //    foreach (DataRow row in dt.Rows)  {
            sb.AppendLine(hasNamespace + "        foreach (DataRow row in dt.Rows)  {");
            //        list.Add(ToModel(row));
            sb.AppendLine(hasNamespace + "            list.Add(ToModel(row));");
            //    }
            sb.AppendLine(hasNamespace + "        }");
            //    return list;
            sb.AppendLine(hasNamespace + "        return list;");
            //}
            sb.AppendLine(hasNamespace + "    }");
            sb.AppendLine("");
        }

        //生成Insert
        public void CreateInsert(string tableName, DataTable dt, StringBuilder sb, string hasNamespace)
        {
            string[] colNames = GetColumnNames(dt);
            sb.AppendLine(hasNamespace + "    /// <summary>");
            sb.AppendLine(hasNamespace + "    /// 插入一条记录");
            sb.AppendLine(hasNamespace + "    /// </summary>");
            sb.AppendLine(hasNamespace + "    /// <param name=\"model\">" + tableName + "类的对象</param>");
            sb.AppendLine(hasNamespace + "    /// <returns>插入是否成功</returns>");
            //            public int Insert(Role model) {
            sb.AppendLine(hasNamespace + "    public bool Insert(" + tableName + " model) {");
            //    SqlHelper.ExecuteNonQuery(
            //        "INSERT INTO Role(RoleID,RoleName,AdderID,AddIP,AddTime,ModifierID,ModifyIP,ModifyTime) VALUES (@RoleID,@RoleName,@AdderID,@AddIP,@AddTime,@ModifierID,@ModifyIP,@ModifyTime);SELECT @@identity"
            sb.AppendLine(hasNamespace + "        int count = SqlHelper.ExecuteNonQuery(@\"INSERT INTO " + tableName + "(" + string.Join(", ", colNames) + ") VALUES(@" + string.Join(", @", colNames) + ")\"");
            //        ,new SqlParameter("@RoleID", model.RoleID)
            GetSqlParameter(dt, sb, hasNamespace);
            //    );
            sb.AppendLine(hasNamespace + "        );");
            sb.AppendLine(hasNamespace + "    return count > 0;");
            //}
            sb.AppendLine(hasNamespace + "    }");
            sb.AppendLine("");
        }

        //生成Update
        public void CreateUpdate(string tableName, DataTable dt, StringBuilder sb, string hasNamespace)
        {
            sb.AppendLine(hasNamespace + "    /// <summary>");
            sb.AppendLine(hasNamespace + "    /// 更新一条记录");
            sb.AppendLine(hasNamespace + "    /// </summary>");
            sb.AppendLine(hasNamespace + "    /// <param name=\"model\">" + tableName + "类的对象</param>");
            sb.AppendLine(hasNamespace + "    /// <returns>更新是否成功</returns>");
            //      public void Update(model model)
            sb.AppendLine(hasNamespace + "    public bool Update(" + tableName + " model) {");
            //    Helper.SqlHelper.ExecuteNonQuery("update T_Operators set UserName=@UserName, RealName=@RealName, Password=@Password where Id=@Id", new SqlParameter("@UserName", userName), new SqlParameter("@RealName", realName), new SqlParameter("@Password", password), new SqlParameter("@Id", id));
            sb.AppendLine(hasNamespace + "        int count = SqlHelper.ExecuteNonQuery(\"UPDATE " + tableName + " SET " + string.Join(", ", GetColumnNamesUpdate(dt)) + " WHERE " + dt.Columns[0].ColumnName + "=@" + dt.Columns[0].ColumnName + "\"");
            GetSqlParameter(dt, sb, hasNamespace);
            sb.AppendLine(hasNamespace + "        );");
            sb.AppendLine(hasNamespace + "    return count > 0;");
            //}
            sb.AppendLine(hasNamespace + "    }");
            sb.AppendLine("");
        }

        //生成GetById
        public void CreateGetById(string tableName, DataTable dt, StringBuilder sb, string hasNamespace)
        {
            sb.AppendLine(hasNamespace + "    /// <summary>");
            sb.AppendLine(hasNamespace + "    /// 获得一条记录");
            sb.AppendLine(hasNamespace + "    /// </summary>");
            sb.AppendLine(hasNamespace + "    /// <param name=\"Id\">主键</param>");
            sb.AppendLine(hasNamespace + "    /// <returns>" + tableName + "类的对象</returns>");
            //public Role Get(int id)
            sb.AppendLine(hasNamespace + "    public " + tableName + " GetById(" + dt.Columns[0].DataType + " " + dt.Columns[0].ColumnName + ") {");
            //    DataTable dt = SqlHelper.ExecuteDataTable("SELECT * FROM Role WHERE ID=@ID", new SqlParameter("@ID", id));
            string[] colNames = GetColumnNames(dt);
            sb.AppendLine(hasNamespace + "        DataTable dt = SqlHelper.ExecuteDataTable(\"SELECT " + string.Join(", ", colNames) + " FROM " + tableName + " WHERE " + dt.Columns[0].ColumnName + "=@" + dt.Columns[0].ColumnName + "\", new SqlParameter(\"@" + dt.Columns[0].ColumnName + "\", " + dt.Columns[0].ColumnName + "));");
            //    if (dt.Rows.Count > 1) {
            sb.AppendLine(hasNamespace + "        if (dt.Rows.Count > 1) {");
            //        throw new Exception("more than 1 row was found");
            sb.AppendLine(hasNamespace + "            throw new Exception(\"more than 1 row was found\");");
            //    }
            sb.AppendLine(hasNamespace + "        }");
            //    if (dt.Rows.Count <= 0)
            sb.AppendLine(hasNamespace + "        else if (dt.Rows.Count <= 0) {");
            //        return null;
            sb.AppendLine(hasNamespace + "            return null;");
            //    }
            sb.AppendLine(hasNamespace + "        }");
            //    DataRow row = dt.Rows[0];
            sb.AppendLine(hasNamespace + "        DataRow row = dt.Rows[0];");
            //    Role model = ToModel(row);
            sb.AppendLine(hasNamespace + "        " + tableName + " model = ToModel(row);");
            //    return model;
            sb.AppendLine(hasNamespace + "        return model;");
            //}
            sb.AppendLine(hasNamespace + "    }");
            sb.AppendLine("");
        }

        //生成DeleteById
        public void CreateDeleteById(string tableName, DataTable dt, StringBuilder sb, string hasNamespace)
        {
            sb.AppendLine(hasNamespace + "    /// <summary>");
            sb.AppendLine(hasNamespace + "    /// 删除一条记录");
            sb.AppendLine(hasNamespace + "    /// </summary>");
            sb.AppendLine(hasNamespace + "    /// <param name=\"Id\">主键</param>");
            sb.AppendLine(hasNamespace + "    /// <returns>删除是否成功</returns>");
            //            public bool Delete(int id)
            sb.AppendLine(hasNamespace + "    public bool DeleteById(" + dt.Columns[0].DataType + " " + dt.Columns[0].ColumnName + ") {");
            //    int rows = SqlHelper.ExecuteNonQuery("DELETE FROM Role WHERE ID = @id", new SqlParameter("@id", id));
            sb.AppendLine(hasNamespace + "        int rows = SqlHelper.ExecuteNonQuery(\"DELETE FROM " + tableName + " WHERE " + dt.Columns[0].ColumnName + " = @" + dt.Columns[0].ColumnName + "\", new SqlParameter(\"@" + dt.Columns[0].ColumnName + "\", " + dt.Columns[0].ColumnName + "));");
            //    return rows > 0;
            sb.AppendLine(hasNamespace + "        return rows > 0;");
            //}
            sb.AppendLine(hasNamespace + "    }");
            sb.AppendLine("");
        }

        //生成ListByWhere
        public void CreateListByWhere(string tableName, StringBuilder sb, string hasNamespace)
        {
            /*
             * public  IEnumerable<T_Roles> ListByWhere(T_Roles model, params string[] fields)
             * {
             *  string str = Helper.GenericSQLGenerator.GetWhereStr<T_Roles>(model,"T_Roles",fields);
             *   List<T_Roles> list = new List<T_Roles>();
             *    DataTable dt = SqlHelper.ExecuteDataTable(str);
             *    foreach (DataRow row in dt.Rows)  {
             *    list.Add(ToModel(row));
             *    }
             *    return list;
             * }
             */
            sb.AppendLine(hasNamespace + "    /// <summary>");
            sb.AppendLine(hasNamespace + "    /// 通过条件获得满足条件的记录");
            sb.AppendLine(hasNamespace + "    /// </summary>");
            sb.AppendLine(hasNamespace + "    /// <param name=\"model\">" + tableName + "类的对象</param>");
            sb.AppendLine(hasNamespace + "    /// <param name=\"fields\">需要的条件的字段名</param>");
            sb.AppendLine(hasNamespace + "    /// <returns>满足条件的记录</returns>");
            sb.AppendLine(hasNamespace + "     public IEnumerable<" + tableName + "> ListByWhere(" + tableName + " model, params string[] fields)");
            sb.AppendLine(hasNamespace + "     {");
            sb.AppendLine(hasNamespace + "         List<SqlParameter> lsParameter = new List<SqlParameter>();");
            sb.AppendLine(hasNamespace + "         string str = Helper.GenericSQLGenerator.GetWhereStr<" + tableName + ">(model, \"" + tableName + "\", out lsParameter, fields);");
            sb.AppendLine(hasNamespace + "         List<" + tableName + "> list = new List<" + tableName + ">();");
            sb.AppendLine(hasNamespace + "         SqlParameter[] sqlparm = new SqlParameter[lsParameter.Count];");
            sb.AppendLine(hasNamespace + "         for (int i = 0; i < lsParameter.Count; i++)");
            sb.AppendLine(hasNamespace + "         {");
            sb.AppendLine(hasNamespace + "             sqlparm[i] = lsParameter[i];");
            sb.AppendLine(hasNamespace + "         }");
            sb.AppendLine(hasNamespace + "         DataTable dt = SqlHelper.ExecuteDataTable(str, sqlparm);");
            sb.AppendLine(hasNamespace + "         foreach (DataRow row in dt.Rows)");
            sb.AppendLine(hasNamespace + "         {");
            sb.AppendLine(hasNamespace + "             list.Add(ToModel(row));");
            sb.AppendLine(hasNamespace + "         }");
            sb.AppendLine(hasNamespace + "         return list;");
            sb.AppendLine(hasNamespace + "     }");
            sb.AppendLine("");
        }
    }
}
