using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Models.DbEntities;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Business
{
    public static class MainController
    {
        private static object _timerLocker = new object();
        private static object _rptBuildTimerLocker = new object();

        private static int _sampleInv;
        private static Timer _timer;
        private static Timer _rptBuildTimer;
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
            _rptBuildTimer = new Timer(TimeSpan.TicksPerMinute/TimeSpan.TicksPerMillisecond);
            _dataSrc = new RedisToDataSource();
            _dnCtrler = new DiagnoseController();
            _rptCtrler = new DiagnoseReportController();
            _dnCtrler.FaultItemHappened += _rptCtrler.BuildFaultItemReports;
            _dnCtrler.InferComboHappened += _rptCtrler.BuildInferComboReports;
            _dnCtrler.MainSpecUpdated += _rptCtrler.BuildMainSpecReports;
        }

        public static void RunProgramLoop()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["SampleInv"], out _sampleInv)) {
                Log.Error("系统初始化失败，数据采集时间未设置，程序退出");
                return;
            }

            var isLocked = false;
            _timer.AutoReset = true;
            _timer.Interval = _sampleInv * 1000;
            _timer.Elapsed += (sender, e) =>
            {       
                lock (_timerLocker) {
                    if (isLocked) {
                        Log.Inform("当前诊断执行尚未完成, 等待下个诊断周期...");
                        return;
                    }
                    isLocked = true;
                }

                _dataSrc.UpdateRtData();
                if (RuntimeRepo.RunningPumpGuids.Any())
                    _dnCtrler.RunDiagnose();


                lock (_timerLocker) {
                    isLocked = false;
                }
            };

            var runMode = Repo.IsHistoryDiagMode ? "历史" : "实时";
            Log.Inform("=============================================", true);
            Log.Inform("        【机泵健康诊断子系统】开始运行", true);
            Log.Inform(true);
            Log.Inform($"当前为: {runMode}诊断模式", true);
            Log.Inform($"泵站代号: {Repo.PSInfo.PSCode}", true);
            Log.Inform($"数据采集时间间隔: {_sampleInv}s", true);
            Log.Inform($"系统分档数: {GradedCriterion.GradeCount}档", true);
            Log.Inform("=============================================", true);

            CreateRunRecord();

            _dataSrc.UpdateRtData();
            _dnCtrler.RunDiagnose();
            _timer.Start();

            LaunchBuildUIReportTask();
        }

        /// <summary>
        /// 开启定时器, 每天指定时间生成ui版的报警报告, 写入机泵监测系统的数据库
        /// </summary>
        public static void LaunchBuildUIReportTask()
        {
            var isLocked = false;
            _rptBuildTimer.AutoReset = true;
            _rptBuildTimer.Elapsed += (sender, e) =>
            {
                var now = DateTime.Now;
                var isTimeToDo = now.Hour == Repo.ReportBuildTime.Hour &&
                                 now.Minute == Repo.ReportBuildTime.Minute;
                if (!isTimeToDo) return;

                lock (_rptBuildTimerLocker) {
                    if (isLocked) {
                        Log.Inform("今日诊断报告正在生成中, 请稍等后重试...");
                        return;
                    }
                    isLocked = true;
                }

                _rptCtrler.BuildUIReport();

                lock (_rptBuildTimerLocker) {
                    isLocked = false;
                }
            };
            _rptBuildTimer.Start();
        }

        public static void ManualBuildUIReport()
        {
            _rptCtrler.BuildUIReport();
        }

        private static void CreateRunRecord()
        {
            var newRRAction = new Action<PumpSystemContext>(context =>
            {
                var rr = new RunRecord {
                    PSGuid = Repo.PSInfo.PSCode,
                    SampleInv = _sampleInv,
                    GradeCount = GradedCriterion.GradeCount,
                    LaunchTime = DateTime.Now
                };
                context.RunRecords.Add(rr);
                context.SaveChanges();
                RuntimeRepo.RRId = rr.Id;
            });

            using (var context = new PumpSystemContext()) {

                //区分历史/实时诊断模式
                if (Repo.IsHistoryDiagMode) {

                    //历史数据的诊断模式下 重启软件认为是数据变更 每次运行作为不同的
                    newRRAction(context);
                } else {

                    //实时数据的诊断模式下 重启软件认为是中断重启 每次运行都使用同一个记录
                    if (context.RunRecords.Any()) {
                        var maxRRId = context.RunRecords.Max(r => r.Id);
                        var rr = context.RunRecords.First(r => r.Id == maxRRId);
                        RuntimeRepo.RRId = rr.Id;

                        rr.RestartTime = DateTime.Now;
                        rr.RestartCount++;
                        context.SaveChanges();
                    } else {
                        newRRAction(context);
                    }
                }

            }
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
