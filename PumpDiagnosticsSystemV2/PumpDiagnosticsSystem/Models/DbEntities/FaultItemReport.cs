using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.Models.DbEntities
{
    [Table("FaultItemReport")]
    public class FaultItemReport : BaseReport
    {
        public DateTime? FirstTime { get; set; }

        public DateTime? LatestTime { get; set; }

        public string CompCode { get; set; }

        public string DisplayText { get; set; }

        public string CriterionBuiltIds { get; set; }

        public string RtDatas { get; set; }

        public SeverityGrade? Severity { get; set; }

        public string Advise { get; set; }

        /// <summary>
        /// 判据中的图表编号 至 数据库中图表的Id 的映射
        /// </summary>
        public string GraphMap { get; set; }

        /// <summary>
        /// 发生次数
        /// </summary>
        public int HappenCount { get; set; }

    }
}
