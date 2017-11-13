using System;
using System.Data;
using System.Data.OleDb;

namespace PumpDiagnosticsSystem.Dbs
{
    public class AccessOp
    {
        private readonly string _path;

        public AccessOp(string path)
        {
            _path = path;
        }

        public static DataSet LoadAllToDataSet(string[] tableNames, string path)
        {
            var ds = new DataSet();
            var connstr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Persist Security Info=False;";
            var conn = new OleDbConnection(connstr);
            conn.Open();
            foreach (var tableName in tableNames) {
                var query = "Select * from " + tableName;
                var adapter = new OleDbDataAdapter(query, conn);
                adapter.Fill(ds, tableName);
            }
            conn.Close();
            return ds;
        }

        /// <summary>
        /// 由于Access中的数据量不大（最多几千条），为了方便，所以一次全部读出再筛选，也不太影响性能；如果数据量大的话，考虑修改查询语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable LoadTable(string tableName)
        {
            var connstr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={_path};Jet OLEDB:Database Password=02154500186";
            var conn = new OleDbConnection(connstr);
            try {
                var ds = new DataSet();
                conn.Open();
                var query = "Select * from " + tableName;
                var adapter = new OleDbDataAdapter(query, conn);
                adapter.Fill(ds, tableName);
                return ds.Tables[tableName];
            } catch (Exception ex) {
                throw new Exception($"无法读取Access表：{tableName},{ex.Message}");
            } finally {
                conn.Close();
            }
        }

//        public DataTable ExecuteQuery(string queryStr)
//        {
//            var connstr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _path;
//            var conn = new OleDbConnection(connstr);
//            try {
//                var ds = new DataSet();
//                conn.Open();
//                var query = "Select * from " + tableName;
//                var adapter = new OleDbDataAdapter(query, conn);
//                adapter.Fill(ds, tableName);
//                return ds.Tables[tableName];
//            } catch (Exception ex) {
//                throw new Exception($"无法读取Access表：{tableName},{ex.Message}");
//            } finally {
//                conn.Close();
//            }
//        }
    }
}
