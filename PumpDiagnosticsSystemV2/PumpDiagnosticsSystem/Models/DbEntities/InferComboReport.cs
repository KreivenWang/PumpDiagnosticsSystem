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

        public string EventMode { get; set; }

        public string DisplayText { get; set; }

        public string Expression { get; set; }

        /// <summary>
        /// A1:1, A2:0, A3:0....
        /// </summary>
        public string RtDatas { get; set; }

        public string Advice { get; set; }

        /// <summary>
        /// 发生次数
        /// </summary>
        public int HappenCount { get; set; }

        /// <summary>
        /// 可信度(发生概率)
        /// </summary>
        public double Credibility { get; set; }

        /// <summary>
        /// 总可信度(总发生概率, 各档相加)
        /// </summary>
        [NotMapped]
        public double TotalCredibility { get; set; }

        /// <summary>
        /// 是否为小概率事件
        /// </summary>
        public bool IsLowProbability { get; set; }


        public int ConvertRemark2ToGrade()
        {
            int g = 0;
            //取Remark2中的第6位
            var gindex = "Grade:n".Length - 1;
            //Remark2不能为空
            if (string.IsNullOrEmpty(Remark2))
                return g;
            //Remark2长度足够
            if (Remark2.Length < gindex)
                return g;
            //解析第6位
            return int.Parse(Remark2.ElementAt(gindex).ToString());
        }

        public string ConvertGradeToColorStr()
        {
            switch (ConvertRemark2ToGrade()) {
                case 0:
                    return "#333333";
                case 1:
                    return "#4169E1";
                case 2:
                    return "#FF6600";
                case 3:
                    return "#FF0000";
            }
            return "#333333";
        }

        public string ConvertGradeToSeverityStr(string prefix = null)
        {
            switch (ConvertRemark2ToGrade()) {
                case 0:
                return string.Empty;
                case 1:
                return prefix + "轻微";
                case 2:
                return prefix + "中度";
                case 3:
                return prefix + "严重";
            }
            return string.Empty;
        }
    }
}
