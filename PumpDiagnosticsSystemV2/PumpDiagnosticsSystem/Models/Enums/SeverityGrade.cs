namespace PumpDiagnosticsSystem.Models
{
    public enum SeverityGrade
    {
        /// <summary>
        /// 不分级，-1
        /// </summary>
        None = -1,

        /// <summary>
        /// 未发生，0
        /// </summary>
        NotHappen = 0,

        /// <summary>
        /// 潜在问题的，1档
        /// </summary>
        Potential = 1,

        /// <summary>
        /// 轻微的，2档
        /// </summary>
        Slight = 2,

        /// <summary>
        /// 严重的，3档
        /// </summary>
        Heavy = 3,

        /// <summary>
        /// 准确的, 4（特殊：用于组合推断为发生的故障）
        /// </summary>
        Precise = 4,
    }
}