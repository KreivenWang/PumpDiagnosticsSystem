using System;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public class NonVibraTransducer : BaseTransducer
    {
        /// <summary>
        /// 传感器编号，因为guid用的是ppguid
        /// </summary>
        public int Number { get; }

        public NonVibraTransducer(Guid ppGuid, CompType type, int num)
            : base(ppGuid, type)
        {
            Number = num;
//            BuildFMEATrees();
        }

        #region Overrides of BaseComponent

        public override string Code =>$"{Number}_{Guid.ToFormatedString()}" ;

        #endregion
    }
}