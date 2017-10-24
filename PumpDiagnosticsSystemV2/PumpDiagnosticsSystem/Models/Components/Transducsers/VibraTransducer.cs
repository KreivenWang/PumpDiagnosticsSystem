using System;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public class VibraTransducer : BaseTransducer
    {
        public VibraTransducer(Guid ssGuid)
            : base(ssGuid, CompType.Td_V)
        {
//            BuildFMEATrees();
        }

        #region Overrides of BaseComponent

        public override string Code => Guid.ToFormatedString();

        public override bool IsDataIsolateSample => true;

        #endregion
    }
}