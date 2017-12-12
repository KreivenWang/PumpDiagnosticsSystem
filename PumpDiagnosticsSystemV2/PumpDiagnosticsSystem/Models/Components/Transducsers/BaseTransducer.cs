using System;
using PumpDiagnosticsSystem.Util;

/*
 * 传感器分为2类，振动传感器、非振动传感器
 * 振动传感器：guid为传感器的ssguid
 * 非振动传感器：guid为泵的ppguid
 */

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public abstract class BaseTransducer : BaseComponent
    {
        public string Signal { get; set; }

        public TdPos? Position { get; set; }

        /// <summary>
        /// 数据更新时间
        /// </summary>
        public DateTime? DataUpdateTime { get; set; }

        protected BaseTransducer(Guid guid, CompType type) 
            : base(guid, type)
        {
        }

        public virtual void BindSignal()
        {
            var prop = Properties.Find(p => p.Variable == Repo.Map.CompDfVar[Type]);
            if (prop != null)
                prop.Value = Signal;
        }
    }
}