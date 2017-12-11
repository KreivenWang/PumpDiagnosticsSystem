using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PumpDiagnosticsSystem.Models
{
    public class Graph
    {
        public DateTime Time { get; set; }

        public TdPos Pos { get; set; }

        public GraphType Type { get; set; }

        public string Signal { get; set; }

        public Guid SSGuid { get; set; }

        public Guid PPGuid { get; set; }

        /// <summary>
        /// 图谱编号，用于判据判断，在添加图谱时设置编号
        /// </summary>
        [NotMapped]
        public int Number { get; set; }
        public List<double> Data { get; } = new List<double>();

        /// <summary>
        /// 包含的波峰线列表
        /// </summary>
        public List<int> PeakLines { get; } = new List<int>();

        public double BandWidth { get; set; }

        public string FeatureStr { get; set; }

        public void UpdateData(double[] datas)
        {
            Data.Clear();
            Data.AddRange(datas);
            for (int i = 1; i < datas.Length; i++) {
                if(i<=1 || i >= datas.Length - 1) continue;
                var curDot = Data[i];
                var prevDot = Data[i - 1];
                var nextDot = Data[i + 1];
                if (curDot > prevDot && curDot > nextDot) {
                    PeakLines.Add(i);
                }
            }
        }
    }

//    public class Spectrum : Graph
//    {
//        
//
//        public Spectrum()
//        {
//            Type = GraphType.Spectrum;
//        }
//    }
//
//    public class TimeWave : Graph
//    {
//        public List<int> Peaks { get; } = new List<int>();
//
//        public Spectrum()
//        {
//            Type = GraphType.Spectrum;
//        }
//    }
}
