using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Datas;
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
            public static double FrequencyWidth { get; } = 1000D;

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
            public static double NoiseAlarmPercent { get; } = 0.01;

            /// <summary>
            /// 判断为噪声的最小宽度百分比(X方向)
            /// </summary>
            public static double NoiseMinWidthPercent { get; } = 1D/3D;

            /// <summary>
            /// 判断为特征点频率的容差
            /// </summary>
            public static double FtJudgeTolerance { get; } = 0.1D;
        }

        public class Dot
        {
            /// <summary>
            /// 编号, 索引
            /// </summary>
            public int Index { get; set; }

            /// <summary>
            /// Hz
            /// </summary>
            public double X { get; set; }

            /// <summary>
            /// mm/s
            /// </summary>
            public double Y { get; set; }

            /// <summary>
            /// 特征值的倍数
            /// </summary>
            public List<FeatureInfo> Features  { get; } = new List<FeatureInfo>();

            public Dot(int index, double y)
            {
                Index = index;
                Y = y;
            }
        }

        /// <summary>
        /// 特征值名称枚举
        /// </summary>
        public enum FtName
        {
            RPS,
            BPFI,
            BPFO,
            BSF,
            FTF,
            BPF
        }

        public class FeatureInfo
        {
            /// <summary>
            /// 特征值名称
            /// </summary>
            public FtName Name { get; set; }

            /// <summary>
            /// 超限报警值
            /// </summary>
            public double Limit { get; set; }

            /// <summary>
            /// 特征值的倍数
            /// </summary>
            public int Ratio { get; set; }

            public bool IsSameFeature(FeatureInfo ft)
            {
                return Name == ft.Name && Ratio == ft.Ratio;
            }

            public bool IsSameFeature(FtName name, int ratio)
            {
                return Name == name && Ratio == ratio;
            }
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
                public double Width { get; set; } = Const.FrequencyWidth;

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
        /// 主峰和边频带组成的峰群
        /// </summary>
        public class SidePeakGroup
        {
             
        }

        #endregion

        #region properties

        public Guid PPGuid { get; }

        public Guid SSGuid { get; }

        /// <summary>
        /// 每秒钟的转速(Hz) Revelutions Per Second
        /// </summary>
        public double RPS { get; }

        /// <summary>
        /// 每分钟的转速(Hz) Revelutions Per Minute
        /// </summary>
        public double RPM { get; }

        /// <summary>
        /// 这里DotList的索引和Dot本身的Index保持一致
        /// </summary>
        public List<Dot> Dots { get; } = new List<Dot>();

        /// <summary>
        /// 图谱的位置
        /// </summary>
        public Position Pos { get; } = new Position();

        public Axis.X AxisX { get; } = new Axis.X();

        public Axis.Y AxisY { get; } = new Axis.Y();

        /// <summary>
        /// 主振频率点
        /// </summary>
        public Dot MVDot { get; private set; }

        /// <summary>
        /// 波峰所在点列表
        /// </summary>
        public List<Dot> PeakDots { get; private set; } = new List<Dot>();

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

        /// <summary>
        /// 特征值对应的转速系数表
        /// </summary>
        public Dictionary<FtName, double> FtDict { get; } = new Dictionary<FtName, double>(6);

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

        public Spectrum(Guid ppGuid, Guid ssGuid, double rpm, IEnumerable<double> data, TdPos tdPos)
        {
            PPGuid = ppGuid;
            SSGuid = ssGuid;
            RPM = rpm;
            RPS = RPM/60; //每分钟转换成每秒钟的转速
            AxisX.LineCount = data.Count();
            SetDots(data);
            SetFtDict();
            SetDotFeatures();
            ParseTdPosToPosition(tdPos);

            SetPeakDots();
            SetMainVibraDot();
            SetZero();
            SetNoiseRegions();
        }

        private void SetDots(IEnumerable<double> data)
        {
            var dataArray = data.ToArray();
            for (int i = 0; i < dataArray.Length; i++) {
                var dot = new Dot(i, dataArray[i]);
                dot.X = dot.Index*AxisX.Dpl;
                Dots.Add(dot);
            }
        }

        private void SetFtDict()
        {
            FtDict.Clear();

            //RPS
            FtDict.Add(FtName.RPS, 1);

            //BEARING
            var brInfoDict = (Pos.CompPos == Position.Component.Pump
                ? DataDetailsOp.GetPumpBearingInfos(PPGuid)
                : DataDetailsOp.GetMotorBearingInfos(PPGuid))
                .Where(b => b.Key.Contains(Pos.DriverPos == Position.Driver.In ? "_In" : "_Out"))
                .ToDictionary(b => b.Key.Split('_')[0].Replace("@", string.Empty), b => b.Value);
            
            FtDict.Add(FtName.BPFI, brInfoDict[FtName.BPFI.ToString()]);
            FtDict.Add(FtName.BPFO, brInfoDict[FtName.BPFO.ToString()]);
            FtDict.Add(FtName.BSF, brInfoDict[FtName.BSF.ToString()]);
            FtDict.Add(FtName.FTF, brInfoDict[FtName.FTF.ToString()]);

            //PUMP - BPF
            if (Pos.CompPos == Position.Component.Pump) {
                var fanCount = DataDetailsOp.GetPumpFanCount(PPGuid);
                FtDict.Add(FtName.BPF, fanCount);
            }
        }

        private void SetDotFeatures()
        {
            foreach (var dot in Dots) {
                foreach (var ft in FtDict) {
                    var ratio = dot.X / (ft.Value * RPS);
                    var intRatio = (int)Math.Round(ratio);
                    if (intRatio == 0)
                        continue;

                    //获取报警值
                    //先按照有倍数的找
                    var ftLmtKey = $"{Pos.CompPos}_{ft.Key}_{intRatio}";
                    var lmt = 999999D;
                    if (Repo.SpecFtLimits.ContainsKey(ftLmtKey)) {
                        lmt = Repo.SpecFtLimits[ftLmtKey];
                    } else {
                        //没找到有倍数的,就按默认1倍的找
                        ftLmtKey = $"{Pos.CompPos}_{ft.Key}_1";
                        if (Repo.SpecFtLimits.ContainsKey(ftLmtKey)) {
                            lmt = Repo.SpecFtLimits[ftLmtKey];
                        } else {
                            //再找不到, 添加日志
                            Log.Error($"找不到特征值 {ftLmtKey} 的报警值, 请检查组件数据库");
                        }
                    }

                    //容差为 +- 0.1Hz
                    if (Math.Abs(ratio - intRatio) <= Const.FtJudgeTolerance) {
                        dot.Features.Add(new FeatureInfo {
                            Name = ft.Key,
                            Ratio = intRatio,
                            Limit = lmt
                        });
                    }
                }
            }
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
                if (curDot.Y > prevDot.Y && curDot.Y > nextDot.Y) {
                    PeakDots.Add(curDot);
                }
            }

            //按峰值从高到低排序
            PeakDots = PeakDots.OrderByDescending(p => p.Y).ToList();
        }

        /// <summary>
        /// 设置主频点
        /// </summary>
        public void SetMainVibraDot()
        {
            MVDot = PeakDots[0];
        }

        private void SetZero()
        {
            var minDotY = Dots.Select(d => d.Y).Min();
            //var minDotX = Data.ToList().IndexOf(minDotY);

            Zero = minDotY*Const.ZeroRatio;
        }

        private void SetNoiseRegions()
        {
            //设置噪音分区
            var lastIndex = Const.RegionPartitions.Count - 1;
            for (int i = 0; i < Const.RegionPartitions.Count; i++) {
                var cur = Const.RegionPartitions[i]*RPS;
                if (i != lastIndex) {
                    var next = Const.RegionPartitions[i + 1]*RPS;
                    var region = new FooterNoiseRegion {
                        BeginFrequence = cur,
                        EndFrequence = next
                    };
                    NoiseRegions.Add(region);
                } else {
                    //至FMax
                    var region = new FooterNoiseRegion {
                        BeginFrequence = cur,
                        EndFrequence = FMax(RPM)/60 //统一使用RPS
                    };
                    NoiseRegions.Add(region);
                }
            }

            //寻找噪音段
            var overDotXs = Dots.Where(d => d.Y > AxisY.AlarmValue*Const.NoiseAlarmPercent)
                .Select(d => d.Index).ToList();

            //获取连续的值 组成的列表
            var continuesPairs = PubFuncs.FindContinues(overDotXs);

            // 噪声最小宽度为: 20RPS x 1/3
            var minWidth = NoiseRegions[0].EndFrequence*Const.NoiseMinWidthPercent;
            NoiseIndexes = continuesPairs.Where(p => p.Item2*AxisX.Dpl - p.Item1*AxisX.Dpl >= minWidth).ToList();

        }

        #endregion

        #region public methods

        /// <summary>
        /// 计算范围内的Y值累加值
        /// </summary>
        /// <param name="start">起始频率</param>
        /// <param name="end">结束频率</param>
        /// <returns></returns>
        public double AggregateRange(double start, double end)
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
                result += Dots[point].Y;
            }
            return result;
        }

        public bool High(FeatureInfo ft)
        {
            //找到相同特征值 并且 是波峰 的点
            var matchDot = Dots.FirstOrDefault(d => d.Features.Exists(f=>f.IsSameFeature(ft)) && PeakDots.Contains(d));
            if (matchDot == null) return false;
            return matchDot.Y > ft.Limit;
        }

        #endregion

        #region Obsolete

        private const string _ObsoleteFeatureAlgorithmMessage = @"不再使用该方法计算得出特征值所在点的Y值. 现在的算法为: 计算每个频谱点都可能存在的特征值.";

        /// <summary>
        /// 获取特征值所在点的Y值
        /// </summary>
        /// <param name="ftName"></param>
        /// <returns></returns>
        [Obsolete(_ObsoleteFeatureAlgorithmMessage)]
        public double GetFeatureValue(FtName ftName)
        {
            double result;

            var higherGdtPoint = FeatureLine(ftName);
            var dotStart = higherGdtPoint - 1;
            var dotEnd = higherGdtPoint + 1;

            var gdtStart = Dots[dotStart];
            var gdtMiddle = Dots[higherGdtPoint];
            var gdtEnd = Dots[dotEnd];

            //中间值必须为波峰
            if (gdtMiddle.Y > gdtStart.Y && gdtMiddle.Y > gdtEnd.Y) {
                //result = gdtStart + gdtMiddle + gdtEnd;
                result = gdtMiddle.Y;
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
        [Obsolete(_ObsoleteFeatureAlgorithmMessage)]
        public int FeatureLine(FtName ftName)
        {
            var rpsCoeff = FtDict[ftName];
            var lineLeft = (int)Math.Round(rpsCoeff * RPS / AxisX.Dpl, MidpointRounding.AwayFromZero);
            var lineRight = lineLeft + 1;

            var gdtLeft = Dots[lineLeft];
            var gdtRight = Dots[lineRight];

            var higherLine = gdtRight.Y > gdtLeft.Y ? lineRight : lineLeft;
            return higherLine;
        }

        #endregion

    }
}
