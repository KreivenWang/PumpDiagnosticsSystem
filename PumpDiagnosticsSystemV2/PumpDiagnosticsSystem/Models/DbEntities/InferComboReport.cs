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
    [Table("InferComboReport")]
    public class InferComboReport : BaseReport
    {
        /// <summary>
        /// InferCombo在故障库中的Id
        /// </summary>
        public int LibId { get; set; }

        public DateTime? FirstTime { get; set; }

        public DateTime? LatestTime { get; set; }

        public string CompCode { get; set; }

        public string EventMode { get; set; }

        public string DisplayText { get; set; }

        public string Expression { get; set; }

        /// <summary>
        /// A1:1, A2:0, A3:0....
        /// </summary>
        public string RtDatas { get; set; }

        public string Advise { get; set; }

        /// <summary>
        /// 发生次数
        /// </summary>
        public int HappenCount { get; set; }
    }
}
