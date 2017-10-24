using System;
using System.Collections.Generic;

namespace PumpDiagnosticsSystem.Models
{
    /// <summary>
    /// 故障项
    /// </summary>
    [Serializable]
    public class FaultItem : BaseFaultItem
    {
        public string Description { get; set; }

        public string Advise { get; set; }

        public List<Criterion> Criteria { get; } =  new List<Criterion>();

        public bool IsHappening => Criteria.Exists(ct => ct.IsHappening);

        /// <summary>
        /// 阈值超限判断的字段，现定为同一种<see cref="FaultItem"/>中的判据的阈值字段不会超过1种
        /// </summary>
        public string ThresholdField { get; set; }
    }
}
