using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PumpDiagnosticsSystem.Models
{
    /// <summary>
    /// 判据，对应access组件库中的Criterion表
    /// </summary>
    [Serializable]
    public class Criterion : BaseFaultItem
    {
        /// <summary>
        /// 判据发生的时间
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// 判据模板的Id
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// 判据在access组件库中CriterionToBuild表中的LibID，设置ID只为方便修改判据
        /// </summary>
        public int LibId { get; set; }
        public string Expression { get; set; }
        public string Description { get; set; }
        public string Advise { get; set; }

//        public Dictionary<string, double> RtDataDict { get; } = new Dictionary<string, double>();

        /// <summary>
        /// 判据中用到的实时数据
        /// </summary>
        public string RtDataStr { get; set; }

        /// <summary>
        /// 阈值超限判断所在字段
        /// </summary>
        public string ThresholdField { get; set; }

        /// <summary>
        /// 是否能输出作为诊断结果报告
        /// </summary>
        public bool AsReportResult { get; set; }

        /// <summary>
        /// 参与计算的图谱编号列表
        /// </summary>
        public List<int> GraphNumbers { get; } = new List<int>(); 

        public bool IsHappening { get; set; }

        /// <summary>
        /// 位置标注
        /// </summary>
        public string PosRemark { get; set; }

        public virtual void ResetStatus()
        {
            IsHappening = false;
            RtDataStr = string.Empty;
            GraphNumbers.Clear();
        }
    }


    [Serializable]
    public class GradedCriterion : Criterion
    {
        public static int GradeCount { get; } = int.Parse(ConfigurationManager.AppSettings["GradeCount"]);

        public static int[] ValidGrades { get; } = ConfigurationManager.AppSettings["ValidGrades"].Split(',').Select(int.Parse).ToArray();

        /// <summary>
        /// 用来表示严重度级别的字段
        /// </summary>
        public string ServerityGradeField { get; set; }

        /// <summary>
        /// 发生的严重度级别（n档）（-1表示不分级/ 0表示不发生 / 1：潜在的，2：轻微的，3：严重的）
        /// </summary>
        public SeverityGrade HappeningGrade { get; set; }

        public static void ForEachValidGradeRange(Action<int[]> gradeAction)
        {
            //与可用分档求交集后去重
            List<int[]> gradesList;
            if (GradeCount == 2) {
                gradesList = new List<int[]> {
                    new[] {2, 3}.Intersect(ValidGrades).ToArray(),
                    new[] {3}
                };
            } else {
                gradesList = new List<int[]> {
                    new[] {1, 2, 3}.Intersect(ValidGrades).ToArray(),
                    new[] {2, 3}.Intersect(ValidGrades).ToArray(),
                    new[] {3}
                };
            }

            //去重
            var resultGrades = new List<int[]>();
            foreach (var grades in gradesList) {
                CheckDistinctToAdd(resultGrades, grades);
            }

            foreach (var grades in resultGrades) {
                gradeAction(grades);
            }
        }

        private static void CheckDistinctToAdd(List<int[]> gradesList, int[] newGrades)
        {
            if (!gradesList.Exists(grades => grades.All(newGrades.Contains) && newGrades.All(grades.Contains))) {
                gradesList.Add(newGrades);
            }
        }

        #region Overrides of Criterion

        public override void ResetStatus()
        {
            base.ResetStatus();
            HappeningGrade = SeverityGrade.NotHappen;
        }

        #endregion
    }
}