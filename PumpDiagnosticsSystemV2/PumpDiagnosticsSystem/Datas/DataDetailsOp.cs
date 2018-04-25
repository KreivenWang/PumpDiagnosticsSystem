using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Models.DbEntities;
using PumpDiagnosticsSystem.Models.Enums;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Datas
{
    public static class DataDetailsOp
    {
        /// <summary>
        /// 报表格式所在相对目录
        /// </summary>
        private static readonly string _rptFmtDir = Directory.GetCurrentDirectory() + @"\Assets\Report\";
        private static readonly string _rptPathTable = _rptFmtDir + "table.html";
        private static readonly string _rptPathItem = _rptFmtDir + "item.html";
        private static readonly string _rptPathSeperator = _rptFmtDir + "seperator.html";
        private static StringBuilder _rptFmtTable;
        private static StringBuilder _rptFmtItem;
        private static StringBuilder _rptFmtSepe;


        private static readonly string connStr = ConfigurationManager.ConnectionStrings["PYWSDbContext"].ConnectionString;

        private static readonly SqlOp _sqlOp = new SqlOp(connStr, true);

        public static PumpStationInfo GetPumpStationInfo()
        {
            var result = new PumpStationInfo();

            //从配置文件中读取获取PSCode
            result.PSCode = ConfigurationManager.AppSettings["PSCODE"].ToUpper();
            if (string.IsNullOrEmpty(result.PSCode)) {
                Log.Error("泵站名称未配置");
            }

            var sql =
                $@"SELECT [PSGUID],[PSCODE],[PSNAME]
  FROM [PUMPSTATION]
  WHERE PSCODE = '{result.PSCode}'";

            _sqlOp.ExecuteReaderQuery(sql, reader => {
                while (reader.Read()) {
                    result.PSGuid = Guid.Parse(reader["PSGUID"].ToString());
                    result.PSName = reader["PSNAME"].ToString();
                }
            });
            return result;
        }

        /// <summary>
        /// 获取机组名称
        /// </summary>
        /// <returns></returns>
        public static string GetPumpSysName(Guid ppguid)
        {
            var result = string.Empty;

            var sql =
                $@"SELECT [PPGUID],[PUMPNAME]
  FROM [PUMP]
  WHERE PPGUID = '{ppguid}'";

            _sqlOp.ExecuteReaderQuery(sql, reader => {
                while (reader.Read()) {
                    result = reader["PUMPNAME"].ToString();
                }
            });
            return result;
        }

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

        #region Diag Report Build Part

        public static int BuildUIReport(PumpSystem ppsys, List<InferComboReport> reports)
        {
            var items = (from rpt in reports
                select new {
                    text = rpt.DisplayText,
                    advice = rpt.Advice,
                    firstTime = rpt.FirstTime,
                    latestTime = rpt.LatestTime,
                    credit = rpt.TotalCredibility,
                    color = rpt.ConvertGradeToColorStr(),
                    severity = rpt.ConvertGradeToSeverityStr()
                }).ToList();
            if (!items.Any())
                return 0;

            ReadRptFormats();
            var itemsSb = new StringBuilder();
            var saveToDbAction = new Action<string>(tt =>
            {
                const string timeFmt = "yy年MM月dd日 HH时mm分";
                for (int i = 0; i < items.Count; i++) {
                    var num = i + 1;
                    var item = items[i];
                    var itemAdvice = string.IsNullOrEmpty(item.advice) ? string.Empty : "建议" + item.advice;
                    var newItem = _rptFmtItem.DeepClone()
                        .Replace("[[rpt-num]]", num.ToString())
                        .Replace("[[rpt-fault]]", item.text)
                        .Replace("[[rpt-severity]]", item.severity)
                        .Replace("[[rpt-severity-color]]", item.color)
                        .Replace("[[rpt-time-first]]", item.firstTime.Value.ToString(timeFmt))
                        .Replace("[[rpt-time-latest]]", item.latestTime.Value.ToString(timeFmt))
                        .Replace("[[rpt-percent]]", Math.Round(item.credit*100, 0).ToString())
                        .Replace("[[rpt-advice]]", itemAdvice)
                        ;
                    itemsSb.Append(newItem);
                    if (i != items.Count - 1) {
                        itemsSb.Append(_rptFmtSepe);
                    }
                }

                var contentSb = new StringBuilder(_rptFmtTable.ToString())
                    .Replace("[[rpt-content]]", itemsSb.ToString());

                //保留自然语言表述的写法
                var content_Obsolete = items.Aggregate(string.Empty,
                    (current, item) =>
                        current +
                        $"<span style=\"color:{item.color};font-size:18px;\"><strong>{item.text}{item.severity}</strong></span></br>" +
                        $"{item.firstTime.Value.ToString(timeFmt)} 至 {item.latestTime.Value.ToString(timeFmt)}时间段内有<strong>{item.credit*100}%</strong>的故障率</br>" +
                        $"建议{item.advice}</br></br>");

                var sql =
                    $@"INSERT INTO [dbo].[DIAGREPORT] (DRTITLE, DRDATE,PSGUID,PPGUID,CREATETIME,DRTYPE,DRCONTENT) 
  VALUES('{tt}',GETDATE(),'{GlobalRepo.PSInfo.PSGuid}','{ppsys.Guid.ToFormatedString()}',GETDATE(),3,'{contentSb.ToString()}')";
                _sqlOp.ExecuteNonQuery(sql);
            });

            string title;

            if (Repo.IsHistoryDiagMode) {
                //历史报告为 今天起运行的结果, 结果中的时间为历史时间
                var timeBegin = reports.Min(r => r.FirstTime);
                var timeEnd = reports.Max(r => r.LatestTime);
                title =
                    $"{GlobalRepo.PSInfo.PSName}{ppsys.Name}{timeBegin.Value.ToString("yy年MM月dd日")} - {timeEnd.Value.ToString("yy年MM月dd日")}";
                saveToDbAction(title);
                Log.Inform($"已构建历史诊断报告:{title}");
            } else {
                title = $"{GlobalRepo.PSInfo.PSName}{ppsys.Name}{DateTime.Today.ToString("yy年MM月dd日")}";
                saveToDbAction(title);
                Log.Inform($"已构建今日诊断报告:{title}");
            }
            return 1;
        }

        private static void ReadRptFormats()
        {
            //读一次
            if (_rptFmtTable != null) return;

            _rptFmtTable = new StringBuilder(GetRptFormat(_rptPathTable));
            _rptFmtItem = new StringBuilder(GetRptFormat(_rptPathItem));
            _rptFmtSepe = new StringBuilder(GetRptFormat(_rptPathSeperator));
        }

        private static string GetRptFormat(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs);
            var text = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            //移除换行/缩进/样式中空格
            return text
                .Replace(Environment.NewLine, string.Empty)
                .Replace("  ", string.Empty)
                .Replace(": ", ":");
        }

        #endregion
    }
}