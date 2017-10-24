using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.Models
{
    public class DiagnoseReport : List<DiagnoseResultItem>
    {
        
    }

    /// <summary>
    /// 诊断结果项，保存到数据库的格式
    /// </summary>
    public class DiagnoseResultItem
    {
        [Key]
        public int Id { get; set; }

        public string PPGuid { get; set; }

        /// <summary>
        /// 组件类型，从组件对象中转换
        /// </summary>
        public string CompType { get; set; }

        /// <summary>
        /// 传感器类型，从组件对象中转换
        /// </summary>
        public string TdPos { get; set; }

        /// <summary>
        /// 故障模式
        /// </summary>
        public string EventMode { get; set; }

        public string CriterionIds { get; set; }

        public string Suggestion { get; set; }

        public string RealtimeData { get; set; }

        public string Remark { get; set; }
    }
}
