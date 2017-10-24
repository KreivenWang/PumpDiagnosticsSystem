using System;

namespace PumpDiagnosticsSystem.Models
{
    /// <summary>
    /// ���������
    /// </summary>
    [Serializable]
    public abstract class BaseFaultItem
    {
        public CompType CompType { get; set; }
        public string EventMode { get; set; }

        /// <summary>
        /// �ж�2���������Ƿ��ʾͬ�ֹ���
        /// </summary>
        public bool IsSameFaultItem(BaseFaultItem fItem)
        {
            return CompType == fItem.CompType && EventMode == fItem.EventMode;
        }
    }
}