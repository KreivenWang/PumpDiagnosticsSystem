using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Business
{
    public static class MainController
    {
        private static Timer _timer;
        private static RedisToDataSource _dataSrc;
        private static DiagnoseController _dnCtrler;
        private static DiagnoseReportController _rptCtrler;

        public static void Initialze()
        {
            ConsoleToLogHelper.Initialize();
            Log.Inform("=============================================", true);
            Log.Inform("        【机泵健康诊断子系统】初始化", true);
            Log.Inform("=============================================", true);
            Repo.Initialize();
            BuildPumpSystems();

            _timer = new Timer();
            _dataSrc = new RedisToDataSource();
            _dnCtrler = new DiagnoseController();
            _rptCtrler = new DiagnoseReportController();
            _dnCtrler.FaultItemHappened += _rptCtrler.BuildFaultItemReports;
            _dnCtrler.InferComboHappened += _rptCtrler.BuildInferComboReports;
            _dnCtrler.MainSpecUpdated += _rptCtrler.BuildMainSpecReports;
        }

        public static void RunProgramLoop()
        {
            int sampleInv;
            if (!int.TryParse(ConfigurationManager.AppSettings["SampleInv"], out sampleInv)) {
                Log.Error("系统初始化失败，数据采集时间未设置，程序退出");
                return;
            }

            var isLocked = false;
            _timer.AutoReset = true;
            _timer.Interval = sampleInv*1000;
            _timer.Elapsed += (sender, e) =>
            {
                if (isLocked) {
                    Log.Inform("当前诊断执行尚未完成, 等待下个诊断周期...");
                    return;
                }
                isLocked = true;
                _dataSrc.UpdateRtData();
                if (RuntimeRepo.RunningPumpGuids.Any())
                    _dnCtrler.RunDiagnose();
                isLocked = false;
            };

            Log.Inform("=============================================", true);
            Log.Inform("        【机泵健康诊断子系统】开始运行", true);
            Log.Inform(true);
            Log.Inform($"泵站代号: {Repo.PSInfo.PSCode}", true);
            Log.Inform($"数据采集时间间隔: {sampleInv}s", true);
            Log.Inform($"系统分档数: {GradedCriterion.GradeCount}档", true);
            Log.Inform("=============================================", true);

            _dataSrc.UpdateRtData();
            _dnCtrler.RunDiagnose();
            _timer.Start();
        }

        private static void BuildPumpSystems()
        {
            RuntimeRepo.PumpSysList.Clear();
            foreach (var ppGuid in Repo.PumpGuids) {
                var ppSys = new PumpSystem(ppGuid);
                ppSys.Name = DataDetailsOp.GetPumpSysName(ppGuid);
                RuntimeRepo.PumpSysList.Add(ppSys);
            }
        }
    }
}
