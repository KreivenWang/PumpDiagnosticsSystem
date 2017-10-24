using System;

namespace PumpDiagnosticsSystem.Models
{
    /// <summary>
    /// 故障项基类
    /// </summary>
    [Serializable]
    public abstract class BaseFaultItem
    {
        public CompType CompType { get; set; }
        public string EventMode { get; set; }

        /// <summary>
        /// 判断2个故障项是否表示同种故障
        /// </summary>
        public bool IsSameFaultItem(BaseFaultItem fItem)
        {
            return CompType == fItem.CompType && EventMode == fItem.EventMode;
        }
    }
}