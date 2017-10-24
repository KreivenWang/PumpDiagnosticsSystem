using System.Data;
using System.IO;

namespace PumpDiagnosticsSystem.Dbs
{
    /// <summary>
    /// Access组件库/判据库/配置库
    /// </summary>
    public static class PumpSysLib
    {
        private static readonly string Address = Directory.GetCurrentDirectory() + "\\Assets\\PumpSystemPropertyLib.accdb";

        private static readonly string[] TableNames_Prop = {
            "CommonProperty",
            "IndirectProperty",
            "ConnectorPointProperty",
            "RefProperty"
        };

        private static readonly string[] TableNames_Criterion = {
            "CriterionTemplate",
            "FaultItem",
            "CriterionToBuild",
            "InferCombo",
            "InferHistory"
        };

        private static readonly string[] TableNames_Const = {
            "Const",
            "PHYDEF",
            "PHYDEF_NOVIBRA",
            "SpectrumFeature"
        };

        private static readonly AccessOp _accessOp;

        #region 组件属性

        public static DataTable TableCommonProperty => _accessOp.LoadTable(TableNames_Prop[0]);
        public static DataTable TableIndirectProperty => _accessOp.LoadTable(TableNames_Prop[1]);
        public static DataTable TableConnectorPointProperty => _accessOp.LoadTable(TableNames_Prop[2]);
        public static DataTable TableRefProperty => _accessOp.LoadTable(TableNames_Prop[3]);

        #endregion


        #region 判据和FMEA

        public static DataTable TableCriterionTemplate => _accessOp.LoadTable(TableNames_Criterion[0]);
        public static DataTable TableFaultItem => _accessOp.LoadTable(TableNames_Criterion[1]);
        public static DataTable TableCriterionToBuild => _accessOp.LoadTable(TableNames_Criterion[2]);
        public static DataTable TableInferCombo => _accessOp.LoadTable(TableNames_Criterion[3]);
        public static DataTable TableInferHistory => _accessOp.LoadTable(TableNames_Criterion[4]);

        #endregion


        #region 配置

        public static DataTable TableConst => _accessOp.LoadTable(TableNames_Const[0]);
        public static DataTable TablePHYDEF => _accessOp.LoadTable(TableNames_Const[1]);
        public static DataTable TablePHYDEF_NOVIBRA => _accessOp.LoadTable(TableNames_Const[2]);
        public static DataTable TableSpectrumFeature => _accessOp.LoadTable(TableNames_Const[3]);

        #endregion


        static PumpSysLib()
        {
            _accessOp = new AccessOp(Address);
        }
    }
}
