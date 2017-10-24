using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PumpDiagnosticsSystem.Models.DbEntities
{
    /// <summary>
    /// 主振频率
    /// </summary>
    [Table("MainSpec")]
    public class MainSpec : BaseReport
    {
        public string PPGuid { get; set; }
        public DateTime? FirstTime { get; set; }
        public DateTime? LatestTime { get; set; }

        /// <summary>
        /// 1X?2X?...
        /// </summary>
        public string Feature { get; set; }

        public string Position { get; set; }

        public double MaxValue { get; set; }

        public double LatestValue { get; set; }

    }
}
