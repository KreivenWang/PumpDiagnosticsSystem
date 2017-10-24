using PumpDiagnosticsSystem.Models;

namespace PumpDiagnosticsSystem.Core.Constructor
{
    internal class CriterionToBuild : BaseFaultItem
    {
        public int LibId { get; set; }
        public int TemplateId { get; set; }
        public string PosRemark { get; set; }
        public string _VAR_A { get; set; }
        public string _VAR_B { get; set; }
        public string _VAR_C { get; set; }
        public string _VAR_D { get; set; }
    }
}