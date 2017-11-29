using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Models.DbEntities;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Business
{
    public class DiagnoseController
    {
        public event Action<PumpSystem> FaultItemHappened;
        public event Action<PumpSystem> InferComboHappened;
        public event Action<MainSpec> MainSpecUpdated;

        private readonly CriterionParser _ctParser;
        private readonly InferComboParser _icParser;

        public DiagnoseController()
        {
            _ctParser = new CriterionParser();
            _icParser = new InferComboParser();
        }

        public void RunDiagnose()
        {
            foreach (var guid in RuntimeRepo.RunningPumpGuids) {
                Log.Inform();
                Log.Inform($"********* 开始诊断机组：{guid} *********", true);
                Log.Inform();

                //设置判据判断结果用的 传感器数据更新时间
                _ctParser.TdUpdateTime = RuntimeRepo.PumpSysTimeDict[guid] ?? DateTime.Now;

                //设置需要进行诊断的机泵系统
                RuntimeRepo.DiagnosingPumpSys = RuntimeRepo.PumpSysList.First(ps => GuidExt.IsSameGuid(guid, ps.Guid));

                //开始诊断
                DiagnoseRunningPump_Round1(RuntimeRepo.DiagnosingPumpSys);
                DiagnoseRunningPump_Round2(RuntimeRepo.DiagnosingPumpSys);
                FindMainVibraSpec(RuntimeRepo.DiagnosingPumpSys);
#if EXRTA
                RabbitSend(RuntimeRepo.DiagnosingPumpSys);
#endif
                Log.Inform();
                Log.Inform($"********* 机组：{guid} 诊断结束 ********", true);
                Log.Inform();
            }
        }

        public void DiagnoseRunningPump_Round1(PumpSystem ppSys)
        {
#if EXRTA
            //显示信号量与属性关联的结果， 结果通过robo3T可视化工具查看表格
            MyMongo.BuildPropsPreview(ppSys);
#endif

            var ppSysReport = ppSys.GetReport();

            //对系统中的每一个部件
            foreach (var comp in ppSys) {

                //寻找其对应的判据
                foreach (var ct in comp.GetAllCriteria()) {

                    #region 获取实时数据的字典

                    var filtedRpt = ppSysReport.Where(rptItem =>
                    {
                        var td = comp as BaseTransducer;
                        if (td == null) { //如果不是传感器，直接过滤组件类型
                            return rptItem.CompType == ct.CompType;
                        }
                        //如果是传感器，还要过滤传感器的位置
                        return rptItem.CompType == ct.CompType && rptItem.TdPos == td.Position;
                    }).ToList();


                    //检查不重复后添加到字典
                    var dict = new Dictionary<string, string>();
                    var conflicts = new List<string>();
                    foreach (var reportItem in filtedRpt) {
                        var key = reportItem.Variable;
                        var value = reportItem.Value;
                        if (dict.ContainsKey(key)) {
                            conflicts.Add(key);
                        } else {
                            dict.Add(key, value);
                        }
                    }

                    //提示修改重复项
                    if (conflicts.Any()) {
                        Log.Warn(
                            $"!!!组件{Repo.Map.TypeToEnum.First(t => t.Value == comp.Type).Key} 属性重复(请从access中手动去除)：!!!");
                        foreach (var cfl in conflicts) {
                            Console.Write($"{cfl}, ");
                        }
                        Log.Inform();
                    }

                    #endregion

                    _ctParser.ParseCriterion(ct, comp.Code, dict);
                }
            }

            FaultItemHappened?.Invoke(ppSys);
        }

        public void DiagnoseRunningPump_Round2(PumpSystem ppSys)
        {
            var ppSysAllCts = new List<Criterion>();
            foreach (var comp in ppSys) {
                ppSysAllCts.AddRange(comp.GetAllCriteria());
            }

            var dict = new Dictionary<int, int>();
            foreach (var ct in ppSysAllCts) {
                var key = ct.LibId;
                var value = ct.IsHappening ? 1 : 0;
                if (!dict.ContainsKey(key)) {
                    dict.Add(key, value);
                }
            }

            foreach (var comp in ppSys) {
                foreach (var icItem in comp.InferComboItems) {
                    _icParser.ParseInferComboItem(icItem, dict);
                    var ctLibIds = _icParser.GetCurrentItemCtLibIds();

                    icItem.ExpressionCts.Clear();
                    var ctCopies = ppSysAllCts.Where(ct => ctLibIds.Contains(ct.LibId)).ToList().DeepClone();
                    icItem.ExpressionCts.AddRange(ctCopies);
                }
            }

//            if (RuntimeRepo.InferResultItems.Any()) {
            InferComboHappened?.Invoke(ppSys);
//            }
        }

        public void FindMainVibraSpec(PumpSystem ppSys)
        {
            var specGraphs = RuntimeRepo.RtData.Graphs.Where(g => g.Signal.Contains("Spec")).ToArray();

            //所有频谱中的最大值
            var max = specGraphs.Max(g => g.Data.Max());

            //找到最大值对应的频率是那个频谱
            var graph = specGraphs.First(g => g.Data.Max() == max);

            //找到是频谱的第几线
            var line = (double)graph.Data.IndexOf(max);

            var lineCount = (double)graph.Data.Count - 1;//去掉第一个0
            var f = line * (Repo.SpecConst.BandWidth / lineCount);

            double? speed = GetSpeed(ppSys);

            var featureF = f / (speed / 60);

            var range = 0.12D;
            var featureFRangeStart = featureF * (1 - range);
            var featureFRangeEnd = featureF * (1 + range);
            var feature = string.Empty;
            var featureList = new[] { 0.5, 1, 2, 3, 4 };
            foreach (var fvalue in featureList) {
                if (fvalue > featureFRangeStart && fvalue < featureFRangeEnd) {
                    feature = fvalue + "X";
                }
            }

            var mainSpec = new MainSpec {
                PPGuid = ppSys.Guid.ToFormatedString(),
                FirstTime = graph.Time,
                LatestTime = graph.Time,
                Feature = feature,
                Position = graph.Pos.ToString(),
                LatestValue = max,
                MaxValue = max
            };
            MainSpecUpdated?.Invoke(mainSpec);
        }

        private static double? GetSpeed(PumpSystem ppSys)
        {
            //获取一下转速
            var speedSignal = ppSys.GetReport().First(rpt => rpt.CompType == CompType.Td_S && rpt.Variable == "@Speed").Value;
            var speed = RuntimeRepo.RtData.FindSignalValue(speedSignal);
            return speed;
        }


        private void RabbitSend(PumpSystem ppSys)
        {
            foreach (var graph in RuntimeRepo.RtData.Graphs) {
                var msgs = new List<string>();
                msgs.Add(ppSys.Guid.ToFormatedString());
                msgs.Add(GetSpeed(ppSys)?.ToString() ?? "-1");
                msgs.Add(graph.Time.ToString("yyyy-MM-dd HH:mm:ss"));
                msgs.Add(graph.Pos.ToString());
                msgs.Add(graph.Type.ToString());
                msgs.Add(GraphArchive.FromGraph(graph).DataStr);
                var msg = string.Join("|||", msgs);
                SpectrumMessenger.Send(msg);
            }
        }
    }
}
