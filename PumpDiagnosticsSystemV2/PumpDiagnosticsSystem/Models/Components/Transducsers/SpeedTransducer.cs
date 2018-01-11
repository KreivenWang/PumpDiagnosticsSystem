using System;

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public class SpeedTransducer : BaseTransducer
    {
        /// <summary>
        /// 根据ppGuid格式化转速变送器的代号
        /// </summary>
        /// <param name="ppGuid"></param>
        /// <returns></returns>
        public static string FormatTdSpeedCode(string ppGuid)
        {
            return $"Speed_{ppGuid.ToUpper()}";
        }

        /// <summary>
        /// 根据ppGuid格式化转速变送器的信号量
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
            NameRemark = "转速变送器";
            Position = "Speed";
//            BuildFMEATrees();
        }

        #region Overrides of BaseComponent

        public override string Code => FormatTdSpeedCode(Guid.ToString());

        public override bool IsDataIsolateSample => true;

        #endregion
    }
}