using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PumpDiagnosticsSystem.Models.DbEntities
{
    /// <summary>
    /// 软件运行记录
    /// </summary>
    [Table("RunRecord")]
    public class RunRecord
    {
        [Key]
        public int Id { get; set; }
        public string PSGuid { get; set; }

        /// <summary>
        /// 采集时间间隔：秒
        /// </summary>
        public int SampleInv { get; set; }

        /// <summary>
        /// 判据总分档数
        /// </summary>
        public int GradeCount { get; set; }

        public DateTime? LaunchTime { get; set; }

        public DateTime? RestartTime { get; set; }

        public int RestartCount { get; set; }
    }
}
