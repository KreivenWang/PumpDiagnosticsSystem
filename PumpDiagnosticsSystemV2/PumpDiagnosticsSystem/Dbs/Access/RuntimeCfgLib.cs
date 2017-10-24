//using System.Data;
//using System.IO;
//
//namespace PumpDiagnosticsSystem.Dbs
//{
//    /// <summary>
//    /// Access运行时配置库
//    /// </summary>
//    public static class RuntimeCfgLib
//    {
//        private static readonly string Address = Directory.GetCurrentDirectory() + "\\Assets\\RuntimeCfgLib.accdb";
//
//        private static readonly string[] TableNames = {
//            "Const",
//            "PHYDEF",
//            "PHYDEF_NOVIBRA"
//        };
//
//        private static readonly AccessOp _accessOp;
//
//        public static DataTable TableConst => _accessOp.LoadTable(TableNames[0]);
//        public static DataTable TablePHYDEF => _accessOp.LoadTable(TableNames[1]);
//        public static DataTable TablePHYDEF_NOVIBRA => _accessOp.LoadTable(TableNames[2]);
//
//        static RuntimeCfgLib()
//        {
//            _accessOp = new AccessOp(Address);
//        }
//    }
//}
