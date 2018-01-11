using System;

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public class SpeedTransducer : BaseTransducer
    {
        /// <summary>
        /// ����ppGuid��ʽ��ת�ٱ������Ĵ���
        /// </summary>
        /// <param name="ppGuid"></param>
        /// <returns></returns>
        public static string FormatTdSpeedCode(string ppGuid)
        {
            return $"Speed_{ppGuid.ToUpper()}";
        }

        /// <summary>
        /// ����ppGuid��ʽ��ת�ٱ��������ź���
        /// </summary>
        /// <param name="ppGuid"></param>
        /// <returns></returns>
        public static string FormatTdSpeedSignal(string ppGuid)
        {
            return "$" + FormatTdSpeedCode(ppGuid);
        }

        public SpeedTransducer(Guid ppGuid) 
            : base(ppGuid, CompType.Td_S)
        {
            Signal = FormatTdSpeedSignal(ppGuid.ToString());
            NameRemark = "ת�ٱ�����";
            Position = "Speed";
//            BuildFMEATrees();
        }

        #region Overrides of BaseComponent

        public override string Code => FormatTdSpeedCode(Guid.ToString());

        public override bool IsDataIsolateSample => true;

        #endregion
    }
}