using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.App
{
    public static class PubMembers
    {
        public static string AppVersion { get; } = ConfigurationManager.AppSettings["AppVersion"];
        public static string AppName => "机泵健康诊断子系统 V" + AppVersion;
    }
}
