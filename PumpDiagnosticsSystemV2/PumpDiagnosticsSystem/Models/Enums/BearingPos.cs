using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.Models.Enums
{
    public enum BearingPos
    {
        /// <summary>
        /// 水泵驱动端
        /// </summary>
        PD,

        /// <summary>
        /// 水泵非驱动端
        /// </summary>
        PND,

        /// <summary>
        /// 电机驱动端
        /// </summary>
        MD,

        /// <summary>
        /// 电机非驱动端
        /// </summary>
        MND
    }
}
