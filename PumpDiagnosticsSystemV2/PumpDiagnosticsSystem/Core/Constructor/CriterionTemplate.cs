namespace PumpDiagnosticsSystem.Core.Constructor
{
    internal class CriterionTemplate
    {
        public int Id { get; set; }
        public string Desciption { get; set; }

        /// <summary>
        /// 模板中的函数的名称
        /// </summary>
        public string FuncName { get; set; }

        /// <summary>
        /// 表达式模板
        /// </summary>
        public string ExpressionTemplate { get; set; }

        /// <summary>
        /// 用于表示严重度分级的变量(常量)
        /// </summary>
        public string GradeVar { get; set; }

        /// <summary>
        /// 持续报警需要过滤的帧数
        /// </summary>
        public int FilteCount { get; set; }

        /// <summary>
        /// 阈值超限判断所在字段
        /// </summary>
        public string ThresholdField { get; set; }

        /// <summary>
        /// 是否能输出作为诊断结果报告
        /// </summary>
        public bool AsReportResult { get; set; }
    }
}