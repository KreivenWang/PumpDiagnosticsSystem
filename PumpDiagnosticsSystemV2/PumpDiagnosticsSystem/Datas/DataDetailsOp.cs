using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Models.Enums;

namespace PumpDiagnosticsSystem.Datas
{
    public static class DataDetailsOp
    {
        private static readonly string connStr = ConfigurationManager.ConnectionStrings["PYWSDbContext"].ConnectionString;

        private static readonly SqlOp _sqlOp = new SqlOp(connStr, true);

        public static DataDictionary GetPumpBearingInfos(Guid ppGuid)
        {
            var brPoses = new[] { "PUMPINBTGUID", "PUMPOUTBTGUID" };
            var result = new DataDictionary();
            foreach (var brPos in brPoses) {
                result.AddRange(GetPartBearingInfos(ppGuid, brPos));
            }
            return result;
        }

        public static DataDictionary GetMotorBearingInfos(Guid ppGuid)
        {
            var brPoses = new[] { "MOTORINBTGUID", "MOTOROUTBTGUID" };
            var result = new DataDictionary();
            foreach (var brPos in brPoses) {
                result.AddRange(GetPartBearingInfos(ppGuid, brPos));
            }
            return result;
        }

        private static DataDictionary GetPartBearingInfos(Guid ppGuid, string brPosColName)
        {
            var result = new DataDictionary();
            var posStr = brPosColName.Contains("IN") ? "_In" : "_Out";
            var sql =
                $@"SELECT 
[PUMP].[PPGUID],
[BEARINGTYPE].[BPFO],
[BEARINGTYPE].[BPFI],
[BEARINGTYPE].[BSF],
[BEARINGTYPE].[FTF] 
FROM [PUMP]
INNER JOIN [BEARINGTYPE]
ON [PUMP].[PPGUID] = '{ppGuid}' 
AND [BEARINGTYPE].[BRGUID] = [PUMP].[{brPosColName}]";

            _sqlOp.ExecuteReaderQuery(sql, reader =>
            {
                while (reader.Read()) {
                    var brInfoNames = new[] {"BPFO", "BPFI", "BSF", "FTF"};
                    foreach (var brInfoName in brInfoNames) {
                        var value = double.Parse(reader[brInfoName].ToString());
                        result.Add($"@{brInfoName}{posStr}", value);
                    }
                }
            });

            return result;
        }

        public static int GetPumpFanCount(Guid ppGuid)
        {
            var result = -1;
            var sql =
                $@"SELECT 
[PUMP].[PPGUID],
[PUMPTYPE].[FANCOUNT]
FROM [PUMP]
INNER JOIN [PUMPTYPE]
ON [PUMP].[PPGUID] = '{ppGuid}' 
AND [PUMPTYPE].[PPTGUID] = [PUMP].[PPTGUID]";

            _sqlOp.ExecuteReaderQuery(sql, reader =>
            {
                while (reader.Read()) {
                    result = int.Parse(reader["FANCOUNT"].ToString());
                }
            });
            return result;
        }
    }
}