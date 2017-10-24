using System;
using System.Collections.Generic;
using System.Linq;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public abstract class BaseComponent
    {
        /// <summary>
        /// <para>水泵、电机、联轴器、转速传感器、非振动传感器：泵的Guid</para>
        /// <para>振动传感器：传感器的Guid</para>
        /// <para>要获取Guid的字符，用Guid的扩展方法<see cref="GuidExt.ToFormatedString"/></para>
        /// </summary>
        public Guid Guid { get; protected set; }

        public CompType Type { get; }

        public List<Property> Properties { get; } = new List<Property>();

        /// <summary>
        /// 名称标注
        /// </summary>
        public string NameRemark { get; set; }

        public List<FaultItem> FaultItems { get; } = new List<FaultItem>();

        public List<InferComboItem> InferComboItems { get; } = new List<InferComboItem>();

        public virtual string Code => $"{Type}_{Guid.ToFormatedString()}";

        public virtual bool IsDataIsolateSample => false;

        protected BaseComponent(Guid guid, CompType type)
        {
            Guid = guid;
            Type = type;
            FaultItems = Repo.FaultItems.Where(fi => fi.CompType == Type).ToList().DeepClone();
            InferComboItems = Repo.InferCombos.Where(ic => ic.CompType == Type).ToList().DeepClone();
        }

        public Criterion[] GetAllCriteria()
        {
            var result = new List<Criterion>();
            foreach (var item in FaultItems) {
                result.AddRange(item.Criteria);
            }
            return result.ToArray();
        }
    }

    [Serializable]
    public class Property
    {
        /// <summary>
        /// 属性的中文描述
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 属性值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 属性所表示的变量
        /// </summary>
        public string Variable { get; set; }
    }

    public enum CompType
    {
        #region 部件相关

        Pump,
        Motor,
        Coupler,

        #endregion

        #region 变送器相关

        /// <summary>
        /// 温度变送器
        /// </summary>
        Td_T,

        /// <summary>
        /// 振动变送器
        /// </summary>
        Td_V,

        /// <summary>
        /// 转速变送器
        /// </summary>
        Td_S,

        /// <summary>
        /// 压力变送器
        /// </summary>
        Td_P,

        #endregion

        #region 电气相关

        /// <summary>
        /// 电流表
        /// </summary>
        PA,

        /// <summary>
        /// 电压表
        /// </summary>
        PV,

        /// <summary>
        /// 电机功率/频率传感器
        /// </summary>
        PF

        #endregion
    }
}
