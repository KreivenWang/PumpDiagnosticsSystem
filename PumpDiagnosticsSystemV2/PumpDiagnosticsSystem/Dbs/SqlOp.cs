using System;
using System.Data;
using System.Data.SqlClient;

namespace PumpDiagnosticsSystem.Dbs
{
    public class SqlOp
    {
        protected const string sqlSeverLocalConnInfo = "Data Source=(local);Initial Catalog={0};Integrated Security=True";

        protected const string sqlSeverWepConnInfo = "Data Source={0};Initial Catalog={1};User ID={2};Password={3}";
        private string _connStr;

        /// <summary>
        /// 本地数据库,使用windows验证登录
        /// </summary>
        /// <param name="dbName"></param>
        public SqlOp(string dbName)
        {
            _connStr = string.Format(sqlSeverLocalConnInfo, dbName);
        }

        /// <summary>
        /// 远程数据库
        /// </summary>
        /// <param name="IPaddress"></param>
        /// <param name="baseName"></param>
        /// <param name="userID"></param>
        /// <param name="userPassword"></param>
        public SqlOp(string IPaddress, string baseName, string userID, string userPassword)
        {
            _connStr = string.Format(sqlSeverWepConnInfo, IPaddress, baseName, userID, userPassword);
        }

        public SqlOp(string connectString, bool useCustomConnStr)
        {
            _connStr = connectString;
        }

        public bool CanConnect()
        {
            return ExecuteNonQuery("Select 1") == 1;
        }

        public int ExecuteNonQuery(string sql)
        {
            var result = -1;
            ConnectTodo(conn => {
                using (var sqlcmd = new SqlCommand(sql, conn)) {
                    result = sqlcmd.ExecuteNonQuery();
                }
            });
            return result;
        }

        public void ExecuteReaderQuery(string sql, Action<IDataReader> readerAction)
        {
            ConnectTodo(conn =>
            {
                using (var sqlcmd = new SqlCommand(sql, conn)) {
                    IDataReader reader = sqlcmd.ExecuteReader();
                    readerAction(reader);
                }
            });
        }

        public object ExecuteScalar(string sql)
        {
            object result = null;
            ConnectTodo(conn =>
            {
                using (var sqlcmd = new SqlCommand(sql, conn)) {
                    result = sqlcmd.ExecuteScalar();
                }
            });
            return result;
        }

        private void ConnectTodo(Action<SqlConnection> todo)
        {
            var conn = new SqlConnection(_connStr);
            try {
                conn.Open(); //打开数据库连接
                todo(conn);
            } finally {
                conn.Close(); //关闭数据库，可再打开
                conn.Dispose(); //释放连接，不可再连接
            }
        }
    }
}
