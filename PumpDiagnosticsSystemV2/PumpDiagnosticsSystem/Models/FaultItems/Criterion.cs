using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PumpDiagnosticsSystem.Models
{
    /// <summary>
    /// �оݣ���Ӧaccess������е�Criterion��
    /// </summary>
    [Serializable]
    public class Criterion : BaseFaultItem
    {
        /// <summary>
        /// �оݷ�����ʱ��
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// �о�ģ���Id
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// �о���access�������CriterionToBuild���е�LibID������IDֻΪ�����޸��о�
        /// </summary>
        public int LibId { get; set; }
        public string Expression { get; set; }
        public string Description { get; set; }
        public string Advise { get; set; }

//        public Dictionary<string, double> RtDataDict { get; } = new Dictionary<string, double>();

        /// <summary>
        /// �о����õ���ʵʱ����
        /// </summary>
        public string RtDataStr { get; set; }

        /// <summary>
        /// ��ֵ�����ж������ֶ�
        /// </summary>
        public string ThresholdField { get; set; }

        /// <summary>
        /// �Ƿ��������Ϊ��Ͻ������
        /// </summary>
        public bool AsReportResult { get; set; }

        /// <summary>
        /// ��������ͼ�ױ���б�
        /// </summary>
        public List<int> GraphNumbers { get; } = new List<int>(); 

        public bool IsHappening { get; set; }

        /// <summary>
        /// λ�ñ�ע
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
        /// ������ʾ���ضȼ�����ֶ�
        /// </summary>
        public string ServerityGradeField { get; set; }

        /// <summary>
        /// ���������ضȼ���n������-1��ʾ���ּ�/ 0��ʾ������ / 1��Ǳ�ڵģ�2����΢�ģ�3�����صģ�
        /// </summary>
        public SeverityGrade HappeningGrade { get; set; }

        public static void ForEachValidGradeRange(Action<int[]> gradeAction)
        {
            //����÷ֵ��󽻼���ȥ��
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

            //ȥ��
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