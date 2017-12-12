using System;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public class VibraTransducer : BaseTransducer
    {
        public string PhaseSignal { get; set; }

        public VibraTransducer(Guid ssGuid)
            : base(ssGuid, CompType.Td_V)
        {
//            BuildFMEATrees();
        }

        public static string ConvertSignalToPhaseSignal(string vibraTdSignal)
        {
            return vibraTdSignal.Replace("$V_", "$Phase_");
        }

        #region Overrides of BaseComponent

        public override string Code => Guid.ToFormatedString();

        public override bool IsDataIsolateSample => true;

        public override void BindSignal()
        {
            base.BindSignal();

            //��Ĭ�ϵ����⣬�����øô���������������λ
            var prop = Properties.Find(p => p.Variable == "@Phase");
            if (prop != null)
                prop.Value = PhaseSignal;
        }

        #endregion
    }
}