using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Core
{
    public class Spectrum
    {
        #region classes

        public class Const
        {
            /// <summary>
            /// 判断零位的系数
            /// </summary>
            public static double ZeroRatio { get; } = 1.5D;

            /// <summary>
            /// 频率宽度(Hz)
            /// </summary>
            public static double FrequncyWidth { get; } = 1000D;

            /// <summary>
            /// 报警值,暂时没用到,还是用的access的const表中的#MAX_A_VIBRATOIN
            /// </summary>
            public static double AlarmValue { get; } = 11.42D;

            /// <summary>
            /// 区域分割点列表,需要乘上转速
            /// </summary>
            public static List<double> RegionPartitions { get; } = new List<double> {
                0D,
                20D,
                50D
            };

            /// <summary>
            /// 判断为噪声点的报警值百分比(Y方向)
            /// </summary>
            public static double NoiseAlarmPercent { get; } = 0.02;

            /// <summary>
            /// 判断为噪声的最小宽度百分比(X方向)
            /// </summary>
            public static double NoiseMinWidthPercent { get; } = 1D/3D;
        }

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
            public double RPSCoefficient { get; set; }

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

            public bool IsPumpOutX
                => CompPos == Component.Pump && DirectionPos == Direction.X && DriverPos == Driver.Out;

            public bool IsPumpOutY
                => CompPos == Component.Pump && DirectionPos == Direction.Y && DriverPos == Driver.Out;

            public bool IsPumpOutZ
                => CompPos == Component.Pump && DirectionPos == Direction.Z && DriverPos == Driver.Out;

            public bool IsMotorInX
                => CompPos == Component.Motor && DirectionPos == Direction.X && DriverPos == Driver.In;

            public bool IsMotorInY
                => CompPos == Component.Motor && DirectionPos == Direction.Y && DriverPos == Driver.In;

            public bool IsMotorInZ
                => CompPos == Component.Motor && DirectionPos == Direction.Z && DriverPos == Driver.In;

            public bool IsMotorOutX
                => CompPos == Component.Motor && DirectionPos == Direction.X && DriverPos == Driver.Out;

            public bool IsMotorOutY
                => CompPos == Component.Motor && DirectionPos == Direction.Y && DriverPos == Driver.Out;

            public bool IsMotorOutZ
                => CompPos == Component.Motor && DirectionPos == Direction.Z && DriverPos == Driver.Out;
        }

        public class Axis
        {
            public class X
            {
                public double Width { get; set; } = Const.FrequncyWidth;

                public int LineCount { get; set; }

                /// <summary>
                /// 频谱分辨率
                /// </summary>
                public double Dpl => Width/(LineCount - (LineCount%10)); //例: 如果是801那就设置为800, 如果是800的话还是800
            }

            public class Y
            {
                public double AlarmValue { get; set; } = Const.AlarmValue;
            }
        }

        /// <summary>
        /// 底脚噪音区
        /// </summary>
        public class FooterNoiseRegion
        {
            public double BeginFrequence { get; set; }

            public double EndFrequence { get; set; }

            public double[] Data { get; set; }
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

        #endregion

        #region properties

        /// <summary>
        /// 每秒钟的转速(Hz) Revelutions Per Second
        /// </summary>
        public double RPS { get; }

        /// <summary>
        /// 每分钟的转速(Hz) Revelutions Per Minute
        /// </summary>
        public double RPM { get; }

        public Dictionary<int, double> Dots { get; } = new Dictionary<int, double>();

        public Position Pos { get; } = new Position();

        public Axis.X AxisX { get; } = new Axis.X();

        public Axis.Y AxisY { get; } = new Axis.Y();

        public MainVibraFrequency MVF { get; private set; }

        /// <summary>
        /// 波峰所在点列表
        /// </summary>
        public Dictionary<int, double> PeakDots { get; private set; } = new Dictionary<int, double>();

        public List<FeatureInfo> Features { get; } = new List<FeatureInfo>();

        /// <summary>
        /// 零位，即图谱中y轴最小的值
        /// </summary>
        public double Zero { get; private set; }

        /// <summary>
        /// 底脚噪音区
        /// </summary>
        public List<FooterNoiseRegion> NoiseRegions { get; } = new List<FooterNoiseRegion>();

        /// <summary>
        /// 底脚噪音的起始和结束编号列表
        /// </summary>
        public List<Tuple<int,int>> NoiseIndexes { get; private set; } = new List<Tuple<int, int>>(); 

        #endregion

        #region static methods

        /// <summary>
        /// 根据转速,计算出振动数据采集频率上限
        /// </summary>
        /// 每分钟转速
        /// <returns></returns>
        public static double FMax(double rpm)
        {
            double ratio;
            if (rpm > 1700) {
                ratio = 60D;
            } else if (rpm >= 1400 && rpm <= 1700) {
                ratio = 60D;
            } else if (rpm >= 1100 && rpm < 1400) {
                ratio = 80D;
            } else if (rpm >= 800 && rpm < 1100) {
                ratio = 100D;
            } else if (rpm >= 600 && rpm < 800) {
                ratio = 120D;
            } else {
                ratio = 120D;
            }
            var result = rpm*ratio;
            return result;
        }

        #endregion

        #region ctor

        public Spectrum(double rpm, IEnumerable<double> data, TdPos tdPos)
        {
            RPM = rpm;
            RPS = RPM/60; //每分钟转换成每秒钟的转速

            SetDots(data);
            SetAxisX();
            ParseTdPosToPosition(tdPos);
            SetFeatures();
            SetPeakDots();
            SetMainVibra();
            SetZero();
            SetNoiseRegions();
        }

        private void SetDots(IEnumerable<double> data)
        {
            var dataArray = data.ToArray();
            for (int i = 0; i < dataArray.Length; i++) {
                Dots.Add(i, dataArray[i]);
            }
        }

        private void SetAxisX()
        {
            AxisX.LineCount = Dots.Count;
        }

        /// <summary>
        /// 把传感器的信息转换到频谱的位置信息
        /// </summary>
        private void ParseTdPosToPosition(TdPos pos)
        {
            var posStr = pos.ToString();

            if (posStr.Contains("_Pump")) {
                Pos.CompPos = Position.Component.Pump;
            } else if (posStr.Contains("_Motor")) {
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
                if (!ftNameStr.Contains(compPosStr)) continue;

                var ft = new FeatureInfo();
                ft.Name =
                    (FeatureInfo.FtName) Enum.Parse(typeof (FeatureInfo.FtName), ftNameStr.Replace(compPosStr, "Ft_"));
                var lmtValueStr = row["FtLimit"].ToString();
                foreach (var @const in Repo.Consts) {
                    if (lmtValueStr.Contains(@const.Key))
                        lmtValueStr = lmtValueStr.Replace(@const.Key, @const.Value.ToString());
                }
                ft.Limit = EasyParser.Parse(lmtValueStr);
                ft.RPSCoefficient = double.Parse(row["SpeedCoefficient"].ToString());
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
                var curDot = Dots[i];
                var prevDot = Dots[i - 1];
                var nextDot = Dots[i + 1];
                if (curDot > prevDot && curDot > nextDot) {
                    PeakDots.Add(i, Dots[i]);
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
                Value = maxPeakValue,
                Feature = ftName
            };
        }

        private void SetZero()
        {
            var minDotY = Dots.Values.Min();
            //var minDotX = Data.ToList().IndexOf(minDotY);

            Zero = minDotY*Const.ZeroRatio;
        }

        private void SetNoiseRegions()
        {
            //设置噪音分区
            var lastIndex = Const.RegionPartitions.Count - 1;
            for (int i = 0; i < Const.RegionPartitions.Count; i++) {
                var cur = Const.RegionPartitions[i]*RPM;
                if (i != lastIndex) {
                    var next = Const.RegionPartitions[i + 1]*RPM;
                    var region = new FooterNoiseRegion {
                        BeginFrequence = cur,
                        EndFrequence = next
                    };
                    NoiseRegions.Add(region);
                } else {
                    //至FMax
                    var region = new FooterNoiseRegion {
                        BeginFrequence = cur,
                        EndFrequence = FMax(RPM)
                    };
                    NoiseRegions.Add(region);
                }
            }

            //寻找噪音段
            var overDotXs = Dots.Where(d => d.Value > AxisY.AlarmValue*Const.NoiseAlarmPercent)
                .Select(d => d.Key).ToList();

            //获取连续的值 组成的列表
            var continuesPairs = PubFuncs.FindContinues(overDotXs);

            // 噪声最小宽度为: 20RPS x 1/3
            var minWidth = NoiseRegions[0].EndFrequence*Const.NoiseMinWidthPercent;
            NoiseIndexes = continuesPairs.Where(p => p.Item2*AxisX.Dpl - p.Item1*AxisX.Dpl >= minWidth).ToList();

        }

        #endregion

        #region public methods

        public FeatureInfo MatchFt(FeatureInfo.FtName ftName)
        {
            return Features.Find(f => f.Name == ftName);
        }

        public double Aggregate(double start, double end)
        {
            var result = 0D;
            int startPoint = (int) Math.Round(start*RPS/AxisX.Dpl) - 1;
            int endPoint = (int) Math.Round(end*RPS/AxisX.Dpl) + 1;

            int point = 0;
            if (startPoint > point)
                point = startPoint;
            if (endPoint > Dots.Count)
                endPoint = Dots.Count - 1;

            point++;

            for (; point <= endPoint; point++) {
                result += Dots[point];
            }
            return result;
        }

        public double GetFeatureValue(FeatureInfo.FtName ftName)
        {
            double result;

            var higherGdtPoint = FeatureLine(ftName);
            var dotStart = higherGdtPoint - 1;
            var dotEnd = higherGdtPoint + 1;

            var gdtStart = Dots[dotStart];
            var gdtMiddle = Dots[higherGdtPoint];
            var gdtEnd = Dots[dotEnd];

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
            var rpmCoeff = MatchFt(ftName).RPSCoefficient;
            var lineLeft = (int) Math.Round(rpmCoeff*RPS/AxisX.Dpl, MidpointRounding.AwayFromZero);
            var lineRight = lineLeft + 1;

            var gdtLeft = Dots[lineLeft];
            var gdtRight = Dots[lineRight];

            var higherLine = gdtRight > gdtLeft ? lineRight : lineLeft;
            return higherLine;
        }

        public bool High(FeatureInfo.FtName ftName)
        {
            return GetFeatureValue(ftName) > MatchFt(ftName).Limit;
        }

        #endregion

    }
}
