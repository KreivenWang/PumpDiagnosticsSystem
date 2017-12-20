using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public class InferComboItem : BaseFaultItem
    {
        public int Id { get; set; }

        public string Expression { get; set; }

        public string FaultResult { get; set; }

        public int[] PrevIds { get; set; }

        public bool IsHappening { get; set; }

        public string Advice { get; set; }

        /// <summary>
        /// 表达式中包含的判据libid所对应的判据副本
        /// </summary>
        public List<Criterion> ExpressionCts { get; } = new List<Criterion>();

        public int GetSlightestGrade()
        {
            return ExpressionCts.Where(ct => ct.IsHappening && ct is GradedCriterion).Cast<GradedCriterion>()
                .Min(gct => (int) gct.HappeningGrade);
        }
    }
}
