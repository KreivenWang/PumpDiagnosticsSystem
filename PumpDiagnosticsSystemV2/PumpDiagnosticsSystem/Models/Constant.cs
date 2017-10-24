using System;

namespace PumpDiagnosticsSystem.Models
{
    [Serializable]
    public class Constant
    {
        /// <summary>
        /// 常量名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 常量值
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 常量描述
        /// </summary>
        public string Description { get; set; }

        public Constant(string name, double value, string desc)
        {
            Name = name;
            Value = value;
            Description = desc;
        }
    }
}
