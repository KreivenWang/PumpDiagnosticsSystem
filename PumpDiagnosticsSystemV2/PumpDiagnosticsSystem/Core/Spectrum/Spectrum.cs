using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Util;
using ServiceStack.Common.Extensions;

namespace PumpDiagnosticsSystem.Core
{
    public class Spectrum
    {
        public class FeatureInfo
        {
            public enum FtName
            {
                Ft_1X,
                Ft_2X,
                Ft_3X,
            }

            /// <summary>
            /// 特征值名称
            /// </summary>
            public FtName Name { get; set; }

            /// <summary>
            /// 超限报警值
            /// </summary>
            public double Limit { get; set; }

            /// <summary>
            /// 计算所在频率线时, 转速前的系数
            /// </summary>
            public double SpeedCoefficient { get; set; }

            /// <summary>
            /// 所在线
            /// </summary>
            public int Line { get; set; }

            public string Description { get; set; }
        }

        public class Position
        {
            public enum Component
            {
                Pump,
                Motor
            }

            public enum Direction
            {
                X,
                Y,
                Z
            }

            public enum Driver
            {
                In,
                Out
            }

            public Component CompPos { get; set; }
            public Direction DirectionPos { get; set; }
            public Driver DriverPos { get; set; }

            public bool IsPumpInX => CompPos == Component.Pump && DirectionPos == Direction.X && DriverPos == Driver.In;
            public bool IsPumpInY => CompPos == Component.Pump && DirectionPos == Direction.Y && DriverPos == Driver.In;
            public bool IsPumpInZ => CompPos == Component.Pump && DirectionPos == Direction.Z && DriverPos == Driver.In;

            public bool IsPumpOutX => CompPos == Component.Pump && DirectionPos == Direction.X && DriverPos == Driver.Out;
            public bool IsPumpOutY => CompPos == Component.Pump && DirectionPos == Direction.Y && DriverPos == Driver.Out;
            public bool IsPumpOutZ => CompPos == Component.Pump && DirectionPos == Direction.Z && DriverPos == Driver.Out;

            public bool IsMotorInX => CompPos == Component.Motor && DirectionPos == Direction.X && DriverPos == Driver.In;
            public bool IsMotorInY => CompPos == Component.Motor && DirectionPos == Direction.Y && DriverPos == Driver.In;
            public bool IsMotorInZ => CompPos == Component.Motor && DirectionPos == Direction.Z && DriverPos == Driver.In;

            public bool IsMotorOutX => CompPos == Component.Motor && DirectionPos == Direction.X && DriverPos == Driver.Out;
            public bool IsMotorOutY => CompPos == Component.Motor && DirectionPos == Direction.Y && DriverPos == Driver.Out;
            public bool IsMotorOutZ => CompPos == Component.Motor && DirectionPos == Direction.Z && DriverPos == Driver.Out;
        }

        public class Axis
        {
            public class X
            {
                public double Width { get; set; } = 1000D;

                public int LineCount { get; set; }

                /// <summary>
                /// 频率分辨率
                /// </summary>
                public double Dpl => Width /(LineCount - (LineCount % 10)); //例: 如果是801那就设置为800, 如果是800的话还是800
            }

            public class Y
            {
                public double MaxVibration { get; set; } = 11.42D;
            }
        }

        /// <summary>
        /// 主振频率
        /// </summary>
        public struct MainVibraFrequency
        {
            /// <summary>
            /// 这个值的赋值没测过,写法可能有问题
            /// </summary>
            public FeatureInfo.FtName? Feature { get; set; }

            public double Value { get; set; }
        }

        public Dictionary<string, double> FeatureDescs { get; private set; }

        public double Speed { get; }

        public double[] Data { get; }

        public Position Pos { get; } = new Position();

        public Axis.X AxisX { get; } = new Axis.X();

        public Axis.Y AxisY { get; } = new Axis.Y();

        public MainVibraFrequency MVF { get; private set; }

        public double FrequenceInterval => 60*AxisX.Dpl;

        /// <summary>
        /// 波峰所在点列表
        /// </summary>
        public Dictionary<int,double> PeakDots { get; private set; } = new Dictionary<int, double>();
     
        public List<FeatureInfo> Features { get; } = new List<FeatureInfo>(); 

        public Spectrum(double speed, IEnumerable<double> data, TdPos tdPos)
        {
            Speed = speed;
            Data = data.ToArray();
            AxisX.LineCount = Data.Length;
            
            ParseTdPosToPosition(tdPos);
            SetFeatures();
            SetPeakDots();
            SetMainVibra();
        }

        /// <summary>
        /// 把传感器的信息转换到频谱的位置信息
        /// </summary>
        private void ParseTdPosToPosition(TdPos pos)
        {
            var posStr = pos.ToString();

            if (posStr.Contains("_Pump")) {
                Pos.CompPos = Position.Component.Pump;
            }else if (posStr.Contains("_Motor")) {
                Pos.CompPos = Position.Component.Motor;
            }

            if (posStr.Contains("_X")) {
                Pos.DirectionPos = Position.Direction.X;
            } else if (posStr.Contains("_Y")) {
                Pos.DirectionPos = Position.Direction.Y;
            } else if (posStr.Contains("_Z")) {
                Pos.DirectionPos = Position.Direction.Z;
            }

            if (posStr.Contains("_Drived")) {
                Pos.DriverPos = Position.Driver.In;
            } else if (posStr.Contains("_NonDrived")) {
                Pos.DriverPos = Position.Driver.Out;
            }
        }

        private void SetFeatures()
        {
            //需要读数据库+计算来替换特征值的获取
//            FeatureDescs = new Dictionary<string, double> {
//                {"1/2L", 2/8D},
//                {"1/2G", 5/8D},
//                {"1X", 8/8D},
//                {"2X", 16/8D},
//                {"3X", 24/8D},
//                {"BPFI", 6.5}
//            };

            //设置报警值
            foreach (DataRow row in PumpSysLib.TableSpectrumFeature.Rows) {
                var ftNameStr = row["FtName"].ToString();
                var compPosStr = Pos.CompPos.ToString();
                if(!ftNameStr.Contains(compPosStr)) continue;

                var ft = new FeatureInfo();
                ft.Name =(FeatureInfo.FtName) Enum.Parse(typeof(FeatureInfo.FtName), ftNameStr.Replace(compPosStr, "Ft_"));
                var lmtValueStr = row["FtLimit"].ToString();
                foreach (var @const in Repo.Consts) {
                    if (lmtValueStr.Contains(@const.Key))
                        lmtValueStr = lmtValueStr.Replace(@const.Key, @const.Value.ToString());
                }
                ft.Limit = EasyParser.Parse(lmtValueStr);
                ft.SpeedCoefficient = double.Parse(row["SpeedCoefficient"].ToString());
                ft.Description = row["FtDesc"].ToString();
                
                Features.Add(ft);
            }
            foreach (var ft in Features) {
                ft.Line = FeatureLine(ft.Name);
            }
        }

        /// <summary>
        /// 计算峰值点
        /// </summary>
        private void SetPeakDots()
        {
            PeakDots.Clear();
            for (int i = 1; i < AxisX.LineCount; i++) {
                if (i <= 1 || i >= AxisX.LineCount - 1)
                    continue;
                var curDot = Data[i];
                var prevDot = Data[i - 1];
                var nextDot = Data[i + 1];
                if (curDot > prevDot && curDot > nextDot) {
                    PeakDots.Add(i, Data[i]);
                }
            }

            //按峰值从高到低排序
            PeakDots = PeakDots.OrderByDescending(p => p.Value).ToDictionary(o => o.Key, o => o.Value);
        }

        /// <summary>
        /// 计算主频
        /// </summary>
        public void SetMainVibra()
        {
            var maxPeakLine = PeakDots.ElementAt(0).Key;
            var maxPeakValue = PeakDots.ElementAt(0).Value;
            FeatureInfo.FtName? ftName = null;
            foreach (var feature in Features) {
                if (maxPeakLine == feature.Line)
                    ftName = feature.Name;
            }
            MVF = new MainVibraFrequency {
                Value =  maxPeakValue,
                Feature = ftName
            };
        }

        public FeatureInfo MatchFt(FeatureInfo.FtName ftName)
        {
            return Features.Find(f => f.Name == ftName);
        }

        public double Aggregate(double start, double end)
        {
            var result = 0D;
            int startPoint = (int) Math.Round(start*Speed/ FrequenceInterval) - 1;
            int endPoint = (int) Math.Round(end*Speed/ FrequenceInterval) + 1;

            int point = 0;
            if (startPoint > point)
                point = startPoint;
            if (endPoint > Data.Length)
                endPoint = Data.Length - 1;

            point++;

            for (; point <= endPoint; point++) {
                result += Data[point];
            }
            return result;
        }

        public double GetFeatureValue(FeatureInfo.FtName ftName)
        {
            double result;

            var higherGdtPoint = FeatureLine(ftName);
            var dotStart = higherGdtPoint - 1;
            var dotEnd = higherGdtPoint + 1;

            var gdtStart = Data[dotStart];
            var gdtMiddle = Data[higherGdtPoint];
            var gdtEnd = Data[dotEnd];

            //中间值必须为波峰
            if (gdtMiddle > gdtStart && gdtMiddle > gdtEnd) {
                //result = gdtStart + gdtMiddle + gdtEnd;
                result = gdtMiddle;
            } else {
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// 特征值所在线
        /// </summary>
        /// <param name="ftName"></param>
        /// <returns></returns>
        public int FeatureLine(FeatureInfo.FtName ftName)
        {
            var speedCoef =  MatchFt(ftName).SpeedCoefficient;
            var lineLeft = (int)Math.Round(speedCoef * Speed / FrequenceInterval, MidpointRounding.AwayFromZero);
            var lineRight = lineLeft + 1;

            var gdtLeft = Data[lineLeft];
            var gdtRight = Data[lineRight];

            var higherLine = gdtRight > gdtLeft ? lineRight : lineLeft;
            return higherLine;
        }

        public bool High(FeatureInfo.FtName ftName)
        {
            return GetFeatureValue(ftName) > MatchFt(ftName).Limit;
        }
    }
}
