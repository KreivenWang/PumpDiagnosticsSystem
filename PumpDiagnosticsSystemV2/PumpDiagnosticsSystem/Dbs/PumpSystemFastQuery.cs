using System;
using System.Collections.Generic;
using System.Linq;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Models.DbEntities;

namespace PumpDiagnosticsSystem.Dbs
{
    public static class PumpSystemFastQuery
    {
        private static readonly SqlOp _sqlOp = new SqlOp("PumpSystem");

//        private static bool IsGraphsExisted()
//        {
//            var exist = false;
//            _sqlOp.ExecuteReaderQuery("if object_id('Graphs') is not null select 1 else select 0",
//                reader =>
//                {
//                    while (reader.Read()) {
//                        exist = (int) reader[0] == 1;
//                    }
//                });
//            return exist;
//        }
//
//        public static void CreateGraphs()
//        {
//            if (!IsGraphsExisted()) {
//                //原来的： create table Graphs ( ID bigint primary key, DataRowNo float not null, Data float not null
//                //现在Id 就是 DataRowNo
//                _sqlOp.ExecuteNonQuery(@"
//create table Graphs 
//(LibId bigint IDENTITY(1,1) not null primary key, 
//Graph NVARCHAR(MAX) not null,
//)");
//            }
//        }

        public static void SaveBulkData(List<Graph> graphs)
        {
            foreach (var graph in graphs) {
                var gStr = string.Join(",", graph.Data);
                var sql = $"INSERT INTO [Graphs](Graph) VALUES('{gStr}');";
                _sqlOp.ExecuteNonQuery(sql);
            }
        }

        public static List<double> GetBulkData(int id)
        {
            var result = new List<double>();
            var sql = $"select DataStr from Graphs where Id = {id}";
            _sqlOp.ExecuteReaderQuery(sql, reader =>
            {
                while (reader.Read()) {
                    var bulkDataStr = reader[0].ToString();
                    result.AddRange(bulkDataStr.Split(',').Select(double.Parse));
                }
            });
            return result;
        }

        /// <summary>
        /// 获取数据表中当前Id的最大值
        /// </summary>
        public static int GetIdMax()
        {
            const string sql = @"SELECT MAX(LibId) FROM [Graphs]";
            var queryResult = _sqlOp.ExecuteScalar(sql);
            if (queryResult == null) return -1;
            return int.Parse(queryResult.ToString());
        }

        /// <summary>
        /// 获取数据表的记录行数
        /// </summary>
        /// <returns></returns>
        public static int GetDataRowCount()
        {
            const string sql = @"SELECT COUNT(1) FROM [Graphs]";
            return int.Parse(_sqlOp.ExecuteScalar(sql).ToString()) ;
        }

//        public static void UpdateAlarmLog(int id, DateTime time)
//        {
//            
//        }
//
//        public static void InsertAlarmLog(AlarmLog alog)
//        {
//            
//        }
    }
}
