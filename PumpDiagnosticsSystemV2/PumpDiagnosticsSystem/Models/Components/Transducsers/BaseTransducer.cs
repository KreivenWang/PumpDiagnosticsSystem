using System;
using PumpDiagnosticsSystem.Util;

/*
 * ��������Ϊ2�࣬�񶯴����������񶯴�����
 * �񶯴�������guidΪ��������ssguid
 * ���񶯴�������guidΪ�õ�ppguid
 */

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public abstract class BaseTransducer : BaseComponent
    {
        public string Signal { get; set; }

        public TdPos? Position { get; set; }

        /// <summary>
        /// ���ݸ���ʱ��
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