using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Models.DbEntities;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Business
{
    public class DiagnoseReportController
    {
        /// <summary>
        /// 超限范围比例
        /// </summary>
        private readonly double _overLimitRatio = 0.1D;

        private readonly Action<BaseReport> _recordValidGrade = r =>
        {
            r.Remark2 = $"可用分档:{string.Join(",", GradedCriterion.ValidGrades)}";
        };

        public DiagnoseReportController()
        {
            
        }

        public void BuildFaultItemReports(PumpSystem ppSys)
        {
            using (var context = new PumpSystemContext()) {
                var reportsToSave = new List<FaultItemReport>();
                foreach (var comp in ppSys) {
                    foreach (var fItem in comp.FaultItems.Where(fi => fi.IsHappening)) {
                        
                        var passedCts = fItem.Criteria.Where(ct => ct.IsHappening && ct.AsReportResult).ToArray();
                        if(!passedCts.Any()) continue;

                        var newRpt = new FaultItemReport();

                        //设置报告时间
                        var happenTime = passedCts.Max(ct => ct.Time);
                        newRpt.FirstTime = happenTime;
                        newRpt.LatestTime = happenTime;

                        //设置报告组件
                        newRpt.CompCode = comp.Code;

                        //设置报告故障内容和建议
                        
                        newRpt.Advise = fItem.Advise;

                        //设置报告故障严重度
                        newRpt.Severity = GetSeverityFromCriteria(passedCts);

                        newRpt.CriterionBuiltIds = string.Join(Repo.Separator, passedCts.Select(ct => ct.LibId));
                        newRpt.RtDatas = string.Join("|||", passedCts.Select(ct => ct.RtDataStr));

                        //设置显示文本
                        newRpt.DisplayText = fItem.Description;

                        #region 给显示文本添加驱动端/非驱动端位置信息

                        var dict = ParseCriterionRtDataStr(newRpt.RtDatas);
                        var hasPos_drived = dict.Find(d => d.Key.Contains("_In")) != null;
                        var hasPos_nonDrived = dict.Find(d => d.Key.Contains("_Out")) != null;
                        var drivePos = string.Empty;
                        //有的fitem 驱动端/非驱动端/xyz都有, 加上这个位置信息反而看不懂
                        //所以只加 只有in的或只有out的
                        if (hasPos_drived && !hasPos_nonDrived) {
                            drivePos = "(驱动端)";
                        } else if (!hasPos_drived && hasPos_nonDrived) {
                            drivePos = "(非驱动端)";
                        }
                        newRpt.DisplayText += drivePos;

                        #endregion

                        //设置默认发生次数
                        newRpt.HappenCount = 1;

                        newRpt.RRId = RuntimeRepo.RRId;

                       var existRpts =
                            context.FaultItemReports.Where(rptRecord => rptRecord.CompCode == newRpt.CompCode &&
                                                                        rptRecord.DisplayText == newRpt.DisplayText &&
                                                                        rptRecord.CriterionBuiltIds == newRpt.CriterionBuiltIds &&
                                                                        rptRecord.Severity >= newRpt.Severity &&
                                                                        rptRecord.RRId == newRpt.RRId).ToArray();

                        

                        var newReportAction = new Action(() =>
                        {
                            

                            //添加到图谱的映射
                            var gMap = new Dictionary<int, int>();
                            var graphNums = passedCts.SelectMany(ct => ct.GraphNumbers).Distinct().ToArray();
                            foreach (var graphNum in graphNums) {
                                var graph = RuntimeRepo.RtData.Graphs[graphNum - 1]; //graphNum从1开始, graphs数组索引从0开始
                                var ga = GraphArchive.FromGraph(graph);
                                var gaId = context.AddGraph(ga);
                                if (!gMap.ContainsKey(graphNum))
                                    gMap.Add(graphNum, gaId);
                            }
                            newRpt.GraphMap = string.Join(Repo.Separator, gMap.Select(m => $"{m.Key}:{m.Value}"));
                            _recordValidGrade(newRpt);

                            //保存
                            reportsToSave.Add(newRpt);
                        });

                        if (!existRpts.Any()) {
                            newReportAction();
                        } else {
                            //获取数据库中最新的记录
                            var latestTime = existRpts.Max(rr => rr.LatestTime);
                            var latestRpt = existRpts.First(r => r.LatestTime == latestTime);

                            var newDict = ParseCriterionRtDataStr(newRpt.RtDatas);
                            var latestDict = ParseCriterionRtDataStr(latestRpt.RtDatas);

                            //如果newRpt有阈值字段，那就找latestRpt里的阈值进行比较，如果newRpt没有阈值字段，那就不用比了，不存
                            double newMax;
                            if (newDict.TryGetMaxValue(fItem.ThresholdField, out newMax)) {

                                double latestMax;
                                if (latestDict.TryGetMaxValue(fItem.ThresholdField, out latestMax)) {

                                    //如果都有阈值，那就比较有没有超过10%，有：就作为新记录，没有：就更新记录时间
                                    if (newMax > (1+_overLimitRatio) * latestMax) {
                                        newRpt.Remark1 = $"较此表Id为{latestRpt.Id}的故障超限至少{_overLimitRatio*100}%，原值:{latestMax} 新值:{newMax}，因此记录为新故障。";
                                        newReportAction();
                                    } else {
                                        latestRpt.LatestTime = newRpt.FirstTime;
                                        latestRpt.HappenCount++;
                                        _recordValidGrade(latestRpt);
                                    }
                                }
                                //如果newRpt有阈值字段，但latestRpt没有，那就当做新的记录存下来
                                else {
                                    newReportAction();
                                }

                            }
                        }
                    }
                }

                if (reportsToSave.Any()) {
                    foreach (var report in reportsToSave) {
                        report.RecordTime = DateTime.Now;
                    }
                    context.FaultItemReports.AddRange(reportsToSave);
                }
                    

                context.SaveChanges();
            }
        }

        public void BuildInferComboReports(PumpSystem ppSys)
        {
            //计算Ic可信度
            var calcCredibilityAction = new Action<PumpSystemContext, InferComboReport>((context, rpt) =>
            {
                var dr = context.DiagRecords.FirstOrDefault(d => d.IcId == rpt.LibId &&
                                                                 d.CompCode == rpt.CompCode &&
                                                                 d.RRId == rpt.RRId);
                if (dr == null) {
                    rpt.Credibility = 0;
                } else {
                    rpt.Credibility = Math.Round((double) rpt.HappenCount/(double) dr.DiagCount, 5);
                }
            });


            using (var context = new PumpSystemContext()) {
                var reportsToSave = new List<InferComboReport>();
                foreach (var comp in ppSys) {

                    #region 添加FMEA树(Ic)的诊断次数统计

                    var newDrList = new List<DiagRecord>();
                    foreach (var icItem in comp.InferComboItems) {
                        var newDr = new DiagRecord {
                            RRId = RuntimeRepo.RRId,
                            IcId = icItem.Id,//判据库中的Id
                            PSGuid = GlobalRepo.PSInfo.PSCode,
                            CompCode = comp.Code,
                            DiagCount = 1
                        };

                        var existDr = context.DiagRecords.FirstOrDefault(dr => newDr.RRId == dr.RRId &&
                                                                               newDr.IcId == dr.IcId &&
                                                                               newDr.PSGuid == dr.PSGuid &&
                                                                               newDr.CompCode == dr.CompCode);

                        //LINQ to Entities 不识别该方法 IsSameDiagRecord 因此该方法无法转换为存储表达式
                        //var existDr = context.DiagRecords.FirstOrDefault(dr => dr.IsSameDiagRecord(newDr));

                        if (existDr == null) {
                            newDrList.Add(newDr);
                        } else {
                            existDr.DiagCount++;
                        }
                        
                    }
                    context.DiagRecords.AddRange(newDrList);
                    context.SaveChanges();

                    #endregion

                    foreach (var icItem in comp.InferComboItems.Where(ic => ic.IsHappening)) {

                        GradedCriterion.ForEachValidGradeRange(gradeRange =>
                        {

                            var passedGcts =
                                icItem.ExpressionCts.Where(ct => ct is GradedCriterion)
                                    .Where(gct => gct.IsHappening)
                                    .Cast<GradedCriterion>().ToList();

                            //假如range是3的话,那么通过的里面至少有个3,且只能有3,不能有1,2
                            //假如range是2,3的话, 那么通过的里面至少有个2,且只能有2,3
                            //假如range是1,2,3的话, 那么通过的里面至少有个1,且只能有1,2,3

                            var checkPass = passedGcts.Any();
                            checkPass &= passedGcts.Exists(g => (int) g.HappeningGrade == gradeRange[0]);
                            checkPass &= passedGcts.All(g => gradeRange.Contains((int) g.HappeningGrade));

                            if (!checkPass) return;

                            var newRpt = new InferComboReport();

                            //设置报告时间
                            var happenTime = passedGcts.Max(ct => ct.Time);
                            newRpt.LibId = icItem.Id;
                            newRpt.FirstTime = happenTime;
                            newRpt.LatestTime = happenTime;

                            //设置报告组件
                            newRpt.CompCode = comp.Code;

                            //设置报告故障内容和建议
                            //                        var faultItem = Repo.FaultItems.First(fi => fi.IsSameFaultItem(icItem));
                            newRpt.DisplayText = icItem.FaultResult;
                            newRpt.EventMode = icItem.EventMode;
                            newRpt.Expression = icItem.Expression;
                            newRpt.RtDatas = string.Join(Repo.Separator,
                                icItem.ExpressionCts.Select(ct => $"{ct.LibId}:{(ct.IsHappening ? 1 : 0)}"));
                            //                        newRpt.Advise = faultItem.Advise;

                            newRpt.HappenCount = 1;
                            calcCredibilityAction(context, newRpt);

                            //remark2 作为所用分档
                            newRpt.Remark2 = "Grade:" + string.Join(",", gradeRange);

                            //把通过的(为1的)分别是几档写到remark3里
                            newRpt.Remark3 = "GradeRefer: "+ string.Join(Repo.Separator,
                                passedGcts.Select(g => $"{g.LibId}:{(int) g.HappeningGrade}"));

                            newRpt.RRId = RuntimeRepo.RRId;

                            newRpt.Advice = icItem.Advice;

//                            var icReports = context.InferComboReports.ToList();

                            //判断是否存在报告，不存在则添加，存在则更新
                            var existRpts =
                                context.InferComboReports.Where(rptRecord => rptRecord.LibId == newRpt.LibId &&
                                                                             rptRecord.CompCode == newRpt.CompCode &&
                                                                             rptRecord.EventMode == newRpt.EventMode &&
                                                                             rptRecord.Expression == newRpt.Expression &&
                                                                             rptRecord.Remark2 == newRpt.Remark2 &&
                                                                             rptRecord.RRId == newRpt.RRId).ToArray();
                            if (!existRpts.Any()) {
                                // SHOULD Has same GRADE && has prevIds && same sensor position
                                // BUT now we only have prevIds here.
                                var intersects = context.InferComboReports.Select(r => r.LibId).Intersect(icItem.PrevIds).ToList();
                                if (intersects.Any()) {
                                    newRpt.DisplayText += "(小概率)";
                                    newRpt.IsLowProbability = true;
                                }
                                reportsToSave.Add(newRpt);
                            } else {
                                var latestTime = existRpts.Max(rr => rr.LatestTime);
                                var latestRpt = existRpts.First(r => r.LatestTime == latestTime);
                                latestRpt.LatestTime = newRpt.FirstTime;
                                latestRpt.RecordTime=DateTime.Now;
                                latestRpt.HappenCount++;
                                calcCredibilityAction(context, latestRpt);
                            }
                        });
                    }
                }

                if (reportsToSave.Any()) {
                    foreach (var report in reportsToSave) {
                        report.RecordTime = DateTime.Now;
                    }
                    context.InferComboReports.AddRange(reportsToSave);
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// 如果判据分级，从判据得出最高的严重度；如果判据不分级，返回默认的严重度
        /// </summary>
        public SeverityGrade GetSeverityFromCriteria(IEnumerable<Criterion> criteria)
        {
            var gcts = criteria.Where(ct => ct is GradedCriterion).Cast<GradedCriterion>().ToList();
            if (!gcts.Any()) {
                return SeverityGrade.None;
            }
            return (SeverityGrade)gcts.Max(gct => (int) gct.HappeningGrade);
        }

        public DataDictionary ParseCriterionRtDataStr(string ctRtDataStr)
        {
            var result = new DataDictionary();
            var matches = CriterionParser.MatchCriterionRtDataDict(ctRtDataStr);

            foreach (var match in matches) {
                var keyvaluepair = match.Split(':');
                if (keyvaluepair.Length == 2) {
                    var key = keyvaluepair[0];
                    var value = keyvaluepair[1];
                    if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value)) {
                        double v;
                        if (double.TryParse(keyvaluepair[1], out v)) {
                            result.Add(keyvaluepair[0], v);
                        }
                    }
                }
            }
            return result;
        }

        public void BuildMainSpecReports(MainSpec mspec)
        {
            /*
             * 每天一条新的
             * 来的新记录,存
             * 来的相同记录, 更新, 
             * 相比前面60-90天之间有超过30%, remark1
             */
            using (var context = new PumpSystemContext()) {
                var existSpec = context.MainSpecs.Where(s =>
                    s.Feature == mspec.Feature &&
                    s.Position == mspec.Position &&
                    s.CompCode == mspec.CompCode).OrderByDescending(s => s.LatestTime).FirstOrDefault();
                if (existSpec != null) {
                    if (mspec.LatestTime.Value.Date > existSpec.LatestTime.Value.Date) {
                        context.MainSpecs.Add(mspec);
                    } else {
                        existSpec.LatestTime = mspec.LatestTime;
                        existSpec.LatestValue = mspec.LatestValue;
                        if (mspec.MaxValue > existSpec.MaxValue) {
                            existSpec.MaxValue = mspec.MaxValue;
                        }

                        //前一个季度 比较 设置remark1
                        var timeRangeStart = existSpec.LatestTime - TimeSpan.FromDays(90 - 5);
                        var timeRangeEnd = existSpec.LatestTime - TimeSpan.FromDays(90 + 5);
                        var prevSeasonSpecs =
                            context.MainSpecs.Where(s =>
                                s.Feature == mspec.Feature &&
                                s.Position == mspec.Position &&
                                s.CompCode == mspec.CompCode)
                                .Where(s => s.LatestTime >= timeRangeStart && s.LatestTime <= timeRangeEnd)
                                .ToList();
                        if (prevSeasonSpecs.Any()) {
                            var prevSeasonSpecMax = prevSeasonSpecs.Max(s => s.MaxValue);
                            if (existSpec.LatestValue > prevSeasonSpecMax*1.2) {
                                existSpec.Remark1 = "超过了上季度最大值的20%";
                            }
                        }
                    }
                } else {
                    context.MainSpecs.Add(mspec);
                }

                context.SaveChanges();
            }
        }

        public int BuildUIReport()
        {
            var buildCount = 0;
            using (var context = new PumpSystemContext()) {

                foreach (var ppsys in RuntimeRepo.PumpSysList) {

                /*
                 * 列为诊断结果的条件:
                 * 
                 * 当前最后一次的RunRecord中
                 * 
                 * Ic的可信度(发生概率) > 10?%
                 * 
                 * 发生次数 > 10?次
                 * 
                 * 记录的时间为今日起
                 * 
                 * 非小概率事件
                 */
                    var icRpts = context.InferComboReports.Where(r => r.RRId == RuntimeRepo.RRId &&
                                                                      r.Credibility > 0.1 &&
                                                                      r.HappenCount > 10 &&
                                                                      r.RecordTime >= DateTime.Today &&
                                                                      !r.IsLowProbability).ToList();
                    icRpts = icRpts.Where(r => r.CompCode.Contains(ppsys.Guid.ToFormatedString())).ToList();
                    
                    var reportGroups = icRpts.GroupBy(r => new {
                        rrId = r.RRId,
                        compCode = r.CompCode,
                        evMode = r.EventMode,
                        displayText = r.DisplayText
                    });

                    //设置总可信度
                    foreach (var @group in reportGroups) {
                        var totalCredit = @group.Sum(r => r.Credibility);
                        if (totalCredit > 1) totalCredit = 1;
                        foreach (var report in @group) {
                            report.TotalCredibility = totalCredit;
                        }
                    }

                    //保留同一故障中, 最高分档时报的故障
                    icRpts =
                        reportGroups.Select(
                            @group => @group.First(
                                    rpt => rpt.ConvertRemark2ToGrade() == @group.Max(mrpt => mrpt.ConvertRemark2ToGrade())))
                            .ToList();

                    

                    //排序 先按置信度降序排序，再增加故障严重程序降序排序，以严重程序为优先
                    icRpts = icRpts
                        .OrderByDescending(r => r.TotalCredibility)
                        .ThenBy(r => r.ConvertRemark2ToGrade())
                        .ToList();

                    buildCount += DataDetailsOp.BuildUIReport(ppsys, icRpts);
                }
            }
            return buildCount;
        }
    }
}
