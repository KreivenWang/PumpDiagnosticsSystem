using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Models;

namespace PumpDiagnosticsSystem.Util
{
    /// <summary>
    /// 基本的全局数据仓库
    /// </summary>
    public static class GlobalRepo
    {
        public static PumpStationInfo PSInfo { get; private set; }

        static GlobalRepo()
        {
            
        }

        public static void Initialize()
        {
            PSInfo = DataDetailsOp.GetPumpStationInfo();
        }
    }
}
