using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PumpDiagnosticsSystem.Core;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Models;

namespace PumpDiagnosticsSystem.Util
{
    /// <summary>
    /// 用于保存公共的运行时动态数据，由各个Controller来控制其数据变动
    /// </summary>
    public static class RuntimeRepo
    {
        public static event Action DataUpdated;

        /// <summary>
        /// 当前的运行记录Id
        /// </summary>
        public static int RRId { get; set; }

        /// <summary>
        /// 当前正在运行的机泵的Guid
        /// </summary>
        public static List<Guid> RunningPumpGuids { get; } = new List<Guid>();

        /// <summary>
        /// 实时数据
        /// </summary>
        public static RtData RtData { get; set; }

        /// <summary>
        /// 所有的机泵系统
        /// </summary>
        public static List<PumpSystem> PumpSysList { get; } = new List<PumpSystem>();

        /// <summary>
        /// 当前诊断中的机泵系统
        /// </summary>
        public static PumpSystem DiagnosingPumpSys { get; set; }

        /// <summary>
        /// 机泵系统的更新时间的字典
        /// </summary>
        public static Dictionary<Guid, DateTime> PumpSysTimeDict { get; } = new Dictionary<Guid, DateTime>();

        public static SpectrumAnalyser SpecAnalyser { get; } = new SpectrumAnalyser();

        public static double GetRPM(Guid ppGuid)
        {
            return RtData.SpData.FirstOrDefault(d => d.Key.Contains("Speed") && d.Key.Contains(ppGuid.ToFormatedString())).Value;
        }

        public static void InformUpdate()
        {
            DataUpdated?.Invoke();
        }
    }
}
