using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Util;
using Const = PumpDiagnosticsSystem.Util.Repo.SpecConst;

namespace PumpDiagnosticsSystem.Core
{
    public class Spectrum
    {
        #region enums

        /// <summary>
        /// 特征值名称枚举
        /// </summary>
        [Flags]
        public enum FtName
        {
            RPS = 1,
            BPFI = 2,
            BPFO = 4,
            BSF = 8,
            FTF = 16,
            BPF = 32
        }

        [Flags]
        public enum SidePeakGroupType
        {
            BPFI__RPS_FTF = 1,
            BSF__FTF = 2,
            NF__BPFO = 4,
            BPFO__BPFO = 8,
            BPF__RPS = 16,
        }

        /// <summary>
        /// 频段划分
        /// </summary>
        [Flags]
        public enum FreqRegionType
        {
            /// <summary>
            /// 0 - 40倍RPS
            /// </summary>
            Low = 1,

            /// <summary>
            /// 40倍RPS - 50%FMax
            /// </summary>
            Middle = 2,

            /// <summary>
            /// 50%FMax - FMax
            /// </summary>
            High = 4
        }

        /// <summary>
        /// 底脚档位划分
        /// </summary>
        [Flags]
        public enum FooterGradeType
        {
            /// <summary>
            /// 不需要分档
            /// </summary>
            None = 0,
            /// <summary>
            /// 低档底脚
            /// </summary>
            Low = 1,
            /// <summary>
            /// 中档底脚
            /// </summary>
            Middle = 2,
            /// <summary>
            /// 高档底脚
            /// </summary>
            High = 4,
            /// <summary>
            /// 所有档位在一起的判断, 3档加一起 != All, 因为All代表可以跨越3个档位, 而加一起不可以, 所以不要用7(Low+Middle+High)!
            /// </summary>
            All = 8
        }

        #endregion

        #region classes

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
                public double Width { get; set; }

                public int LineCount { get; set; }

                /// <summary>
                /// 频谱分辨率
                /// </summary>
                public double Dpl => Width/LineCount;
            }

            public class Y
            {
                public double AlarmValue { get; set; } = Const.AlarmValue;
            }
        }

        /// <summary>
        /// 底脚噪音区
        /// </summary>
        public class FreqRegion
        {
            public double BeginFrequence { get; set; }

            public double EndFrequence { get; set; }

            public FreqRegionType Type { get; set; }
        }

        /// <summary>
        /// 主峰和边频带组成的峰群
        /// </summary>
        public class SidePeakGroup
        {
            public SidePeakGroupType Type { get; }

            public Dot MainPeak { get; set; }

            public List<Dot> SidePeaks { get; } = new List<Dot>();

            public SidePeakGroup(SidePeakGroupType type, Dot mainPeak)
            {
                Type = type;
                MainPeak = mainPeak;
            }
        }

        /// <summary>
        /// 底脚分档
        /// </summary>
        public class FooterGrade
        {
            public FooterGradeType Type { get; set; }

            /// <summary>
            /// 起始幂
            /// </summary>
            public int BeginPow { get; set; }

            /// <summary>
            /// 结束幂
            /// </summary>
            public int EndPow { get; set; }
        }

        #endregion

        #region properties

        public Guid PPGuid { get; }

        public Guid SSGuid { get; }

        /// <summary>
        /// 实时数据中的图表编号
        /// </summary>
        public int GraphNumber { get; }

        /// <summary>
        /// 每秒钟的转速(Hz) Revolutions Per Second
        /// </summary>
        public double RPS { get; }

        /// <summary>
        /// 每分钟的转速(Hz) Revolutions Per Minute
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
        public List<Dot> PeakDots { get; } = new List<Dot>();

        /// <summary>
        /// 特征值对应的转速系数表
        /// </summary>
        public Dictionary<FtName, double> FtCoeffDict { get; } = new Dictionary<FtName, double>(6);

        /// <summary>
        /// 零位，即图谱中y轴最小的值
        /// </summary>
        public double Zero { get; private set; }

        /// <summary>
        /// 底脚噪音区, 是一个设定的固定范围
        /// </summary>
        public List<FreqRegion> NoiseRegions { get; } = new List<FreqRegion>();

        /// <summary>
        /// 频段的划分, 是一个设定的固定范围
        /// </summary>
        public List<FreqRegion> FreqRegions { get; } = new List<FreqRegion>();

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

        public Spectrum(Guid ppGuid, Guid ssGuid, int graphNum, double rpm, double bandWidth, IEnumerable<double> data, TdPos tdPos)
        {
            //Data Initialize
            PPGuid = ppGuid;
            SSGuid = ssGuid;
            GraphNumber = graphNum;
            RPM = rpm;
            RPS = RPM/60; //每分钟转换成每秒钟的转速
            AxisX.LineCount = data.Count();
            AxisX.Width = bandWidth;
            SetDots(data);
            SetFtDict();
            ParseTdPosToPosition(tdPos);

            //Basic Analyse
            SetPeakDots();
            SetPeakDotFeatures();
            SetMainVibraDot();
            SetZero();
            SetNoiseRegions();
            SetFrequencyRegions();

            //Complex Analyse
            //FindNoises();
            //SetSidePeaksGroups();
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
            FtCoeffDict.Clear();

            //RPS
            FtCoeffDict.Add(FtName.RPS, 1);

            //BEARING
            var brInfoDict = (Pos.CompPos == Position.Component.Pump
                ? DataDetailsOp.GetPumpBearingInfos(PPGuid)
                : DataDetailsOp.GetMotorBearingInfos(PPGuid))
                .Where(b => b.Key.Contains(Pos.DriverPos == Position.Driver.In ? "_In" : "_Out"))
                .ToDictionary(b => b.Key.Split('_')[0].Replace("@", string.Empty), b => b.Value);
            
            FtCoeffDict.Add(FtName.BPFI, brInfoDict[FtName.BPFI.ToString()]);
            FtCoeffDict.Add(FtName.BPFO, brInfoDict[FtName.BPFO.ToString()]);
            FtCoeffDict.Add(FtName.BSF, brInfoDict[FtName.BSF.ToString()]);
            FtCoeffDict.Add(FtName.FTF, brInfoDict[FtName.FTF.ToString()]);

            //PUMP - BPF
            if (Pos.CompPos == Position.Component.Pump) {
                var fanCount = DataDetailsOp.GetPumpFanCount(PPGuid);
                FtCoeffDict.Add(FtName.BPF, fanCount);
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
        }

        /// <summary>
        /// 计算峰值点的特征点
        /// </summary>
        private void SetPeakDotFeatures()
        {
            var notFoundKeys = new List<string>();

            foreach (var ftCoeff in FtCoeffDict) {
                for (int i = 1; i < AxisX.Width; i++) {
                    //转速x特征值系数x倍数
                    var m = RPS*ftCoeff.Value*i;
                    if (m > AxisX.Width) break;
                    var lineLeft = (int) Math.Round(m/AxisX.Dpl, MidpointRounding.AwayFromZero);
                    var lineRight = lineLeft + 1;
                    var matchPeakDot = PeakDots.Find(d => d.Index == lineLeft) ??
                                       PeakDots.Find(d => d.Index == lineRight);
                    if (matchPeakDot != null) {
                        //获取报警值
                        //先按照有倍数的找
                        var ftLmtKey = $"{Pos.CompPos}_{ftCoeff.Key}_{i}";
                        var lmt = 999999D;
                        if (Repo.SpecFtLimits.ContainsKey(ftLmtKey)) {
                            lmt = Repo.SpecFtLimits[ftLmtKey];
                        } else {
                            //没找到有倍数的,就按默认1倍的找
                            ftLmtKey = $"{Pos.CompPos}_{ftCoeff.Key}_1";
                            if (Repo.SpecFtLimits.ContainsKey(ftLmtKey)) {
                                lmt = Repo.SpecFtLimits[ftLmtKey];
                            } else {
                                //找不到的话, 输出到日志
                                notFoundKeys.AddSingle(ftLmtKey);
                            }
                        }

                        matchPeakDot.Features.Add(new FeatureInfo {
                            Name = ftCoeff.Key,
                            Ratio = i,
                            Limit = lmt
                        });
                    }
                }
            }

            notFoundKeys.ForEach(k => Log.Warn($"找不到特征值 {k} 的报警值, 请检查组件数据库"));

            #region //原来按照X的误差值去匹配的算法

            //            foreach (var peakDot in PeakDots) {
            //                foreach (var ftCoeff in FtCoeffDict) {
            //                    var possiblePeakIndex = FeatureLine(ftCoeff.Key);
            //                    var ratio = peakDot.X / (ftCoeff.Value * RPS);
            //                    var intRatio = (int)Math.Round(ratio);
            //                    if (intRatio == 0)
            //                        continue;
            //
            //                    //获取报警值
            //                    //先按照有倍数的找
            //                    var ftLmtKey = $"{Pos.CompPos}_{ftCoeff.Key}_{intRatio}";
            //                    var lmt = 999999D;
            //                    if (Repo.SpecFtLimits.ContainsKey(ftLmtKey)) {
            //                        lmt = Repo.SpecFtLimits[ftLmtKey];
            //                    } else {
            //                        //没找到有倍数的,就按默认1倍的找
            //                        ftLmtKey = $"{Pos.CompPos}_{ftCoeff.Key}_1";
            //                        if (Repo.SpecFtLimits.ContainsKey(ftLmtKey)) {
            //                            lmt = Repo.SpecFtLimits[ftLmtKey];
            //                        } else {
            //                            //再找不到, 添加日志
            //                            Log.Error($"找不到特征值 {ftLmtKey} 的报警值, 请检查组件数据库");
            //                        }
            //                    }
            //
            //                    //容差为 +- 0.1Hz
            //                    if (Math.Abs(ratio - intRatio) <= Const.FtJudgeTolerance) {
            //                        peakDot.Features.Add(new FeatureInfo {
            //                            Name = ftCoeff.Key,
            //                            Ratio = intRatio,
            //                            Limit = lmt
            //                        });
            //                    }
            //                }
            //            }

            #endregion
        }

        /// <summary>
        /// 设置主频点
        /// </summary>
        public void SetMainVibraDot()
        {
            MVDot = PeakDots.OrderByDescending(p => p.Y).First();
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
            var lastIndex = Const.NoiseRegionPartitions.Count - 1;
            for (int i = 0; i < Const.NoiseRegionPartitions.Count; i++) {
                var cur = Const.NoiseRegionPartitions[i]*RPS;
                if (i != lastIndex) {
                    var next = Const.NoiseRegionPartitions[i + 1]*RPS;
                    var region = new FreqRegion {
                        BeginFrequence = cur,
                        EndFrequence = next
                    };
                    NoiseRegions.Add(region);
                } else {
                    //至FMax
                    var region = new FreqRegion {
                        BeginFrequence = cur,
                        EndFrequence = FMax(RPM)/60 //统一使用RPS
                    };
                    NoiseRegions.Add(region);
                }
            }
        }

        private void SetFrequencyRegions()
        {
            var fMax = FMax(RPM)/60;//统一使用RPS

            var rpsRegions = new List<Tuple<double, double, FreqRegionType>> {
                new Tuple<double, double, FreqRegionType>(0D, Const.FreqRegion_LowToMiddle*RPS, FreqRegionType.Low),
                new Tuple<double, double, FreqRegionType>(Const.FreqRegion_LowToMiddle*RPS, Const.FreqRegion_MiddleToHigh*fMax, FreqRegionType.Middle),
                new Tuple<double, double, FreqRegionType>(Const.FreqRegion_MiddleToHigh*fMax, fMax, FreqRegionType.High)
            };

            foreach (var rpsRegion in rpsRegions) {
                FreqRegions.Add(new FreqRegion {
                    BeginFrequence = rpsRegion.Item1,
                    EndFrequence = rpsRegion.Item2,
                    Type = rpsRegion.Item3
                });
            }
        }

        public List<Tuple<int, int>> FindDefaultNoises()
        {
            return FindNoises(FooterGradeType.All);
        }

        /// <summary>
        /// 根据指定比例, 获取底脚噪音的编号列表(包含起始和结束编号)
        /// </summary>
        /// <param name="footerGradeType"></param>
        /// <param name="autoGrade">自动档的值</param>
        public List<Tuple<int, int>> FindNoises(FooterGradeType footerGradeType, int? autoGrade = null)
        {
            var result = new List<Tuple<int, int>>();

            //自动档最大分档
            var maxAutoGrade = GradedCriterion.GradeCount;

            //如果自动档是默认值, 即不分自动档, 则按自动档最大值来计算
            if (autoGrade == null)
                autoGrade = maxAutoGrade;

            var minPow = Const.NoiseJudge_Pow_Min;
            var maxPow = Const.NoiseJudge_Pow_Max;

            //幂档范围
            var powRange = maxPow - minPow;

            //幂档步长 //这里3的意思是分low middle high3大档
            var powStep = powRange / 3;

            //(当前自动档 除以 自动档最大值) 作为系数, 乘上 具体的幂档范围 就是具体的自动档值
            //档位从高到低算, 因为范围广的通过了, 才判断范围窄的
            var coeff = (3D - autoGrade.Value + 1D) /3D; //这里3的意思是档位最大值, **后面应改用appconfig的值

            double beginPow;
            double endPow;
            switch (footerGradeType) {
                case FooterGradeType.None:
                    return result;
                case FooterGradeType.Low:
                    beginPow = powStep*minPow;
                    endPow = beginPow + coeff*powStep;
                    break;
                case FooterGradeType.Middle:
                    beginPow = powStep*(minPow + 1);
                    endPow = beginPow + coeff*powStep;
                    break;
                case FooterGradeType.High:
                    beginPow = powStep*(minPow + 2);
                    endPow = beginPow + coeff*powStep;
                    break;
                case FooterGradeType.All:
                    beginPow = powStep*minPow;
                    endPow = coeff*powRange;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var alarmPercentValue = AxisY.AlarmValue*Const.NoiseAlarmPercent;

            //寻找噪音段
            var overDotXs =
                this.Dots.Where(d => d.Y >= alarmPercentValue* Math.Pow(Const.NoiseAlarmJudge_Base, beginPow) &&
                                d.Y <= alarmPercentValue* Math.Pow(Const.NoiseAlarmJudge_Base, endPow))
                    .Select(d => d.Index).ToList();

            //获取连续的值 组成的列表
            var continuesPairs = PubFuncs.FindContinues(overDotXs);

            // 噪声最小宽度为: 20RPS x 1/3
            var minWidth = NoiseRegions[0].EndFrequence * Const.NoiseMinWidthPercent;

            //60多好像太宽了 1个都没有, 设个6吧
            //6个也太宽, 测试用 2个?
//            minWidth = 2;

            result = continuesPairs.FindAll(p => p.Item2 * AxisX.Dpl - p.Item1 * AxisX.Dpl >= minWidth);

            return result;
        }

        public List<SidePeakGroup> FindSidePeaksGroups()
        {
            var result = new List<SidePeakGroup>();

            var findSidePeakGroupsAction = new Action<SidePeakGroupType, List<Dot>, List<Dot>>
                ((spGroupType, mainPeaks, sidePeaks) =>
                {
                    mainPeaks = FilteIncisivePeaks(mainPeaks);
                    sidePeaks = FilteIncisivePeaks(sidePeaks);
                    foreach (var mainPeak in mainPeaks) {

                        //峰值要大于 当地的峰值(前后50个峰值)的平均值 的1.5倍, 首先满足这个条件才算作边频带的主峰
                        var mpIndex = PeakDots.IndexOf(mainPeak);
                        var localStart = mpIndex - Const.SPG_LocalRange;
                        if (localStart < 0)
                            localStart = 1;
                        var localEnd = mpIndex + Const.SPG_LocalRange;
                        if (localEnd > PeakDots.Count - 1)
                            localEnd = PeakDots.Count - 1;
                        var localPeaks = PeakDots.Skip(localStart).Take(localEnd - localStart).ToList();
                        var localAverage = localPeaks.Average(d => d.Y);
                        if (mainPeak.Y > localAverage* Const.SPG_PeakOverRatio) {

                            var curPeakIndex = PeakDots.IndexOf(mainPeak);
                            var peakRange = 2;
                            for (int i = -peakRange; i <= peakRange; i++) {
                                if (i == 0)
                                    continue;
                                if (!PeakDots.ContainsIndex(curPeakIndex + i))
                                    continue;
                                var possibleSidePeak = PeakDots[curPeakIndex + i];
                                if (sidePeaks.Contains(possibleSidePeak)) {
                                    var foundGroup = result.Find(r => r.MainPeak == mainPeak);
                                    if (foundGroup != null) {
                                        foundGroup.SidePeaks.Add(possibleSidePeak);
                                    } else {
                                        var group = new SidePeakGroup(spGroupType, mainPeak);
                                        group.SidePeaks.Add(possibleSidePeak);

                                        //边频带峰的个数要大于等于一定数量
                                        if (group.SidePeaks.Count >= Const.SPG_SpCount) {
                                            result.Add(group);
                                        }
                                    }
                                }
                            }
                        }
                    }
                });

            //main: BPFI  side: RPS/FTF
            var mps = PeakDots.FindAll(d => d.Features.Exists(f => f.Name == FtName.BPFI));
            var sps = PeakDots.FindAll(d => d.Features.Exists(f => f.Name == FtName.RPS || f.Name == FtName.FTF));
            findSidePeakGroupsAction(SidePeakGroupType.BPFI__RPS_FTF, mps, sps);

            //main: BSF  side: FTF
            mps = PeakDots.FindAll(d => d.Features.Exists(f => f.Name == FtName.BSF));
            sps = PeakDots.FindAll(d => d.Features.Exists(f => f.Name == FtName.FTF));
            findSidePeakGroupsAction(SidePeakGroupType.BSF__FTF, mps, sps);

            //main: Nothing(Natural Frequency)  side: BPFO
            mps = PeakDots.FindAll(d => d.Features.Exists(f => !d.Features.Any()));
            sps = PeakDots.FindAll(d => d.Features.Exists(f => f.Name == FtName.BPFO));
            findSidePeakGroupsAction(SidePeakGroupType.NF__BPFO, mps, sps);

            //main: BPFO  side: BPFO
            mps = PeakDots.FindAll(d => d.Features.Exists(f => f.Name == FtName.BPFO));
            sps = PeakDots.FindAll(d => d.Features.Exists(f => f.Name == FtName.BPFO));
            findSidePeakGroupsAction(SidePeakGroupType.BPFO__BPFO, mps, sps);

            //main: BPF  side: RPS
            mps = PeakDots.FindAll(d => d.Features.Exists(f => f.Name == FtName.BPF));
            sps = PeakDots.FindAll(d => d.Features.Exists(f => f.Name == FtName.RPS));
            findSidePeakGroupsAction(SidePeakGroupType.BPF__RPS, mps, sps);

            return result;
        }

        /// <summary>
        /// 从原始峰值列表中过滤出尖锐的峰值
        /// </summary>
        /// <param name="originDots"></param>
        /// <returns></returns>
        private List<Dot> FilteIncisivePeaks(List<Dot> originDots)
        {
            var r = new List<Dot>();
            var minIndex = 0;
            var maxIndex = Dots.Count - 1;
            foreach (var dot in originDots) {
                var prevId1 = dot.Index - 1;
                var prevId2 = dot.Index - 2;
                var nextId1 = dot.Index + 1;
                var nextId2 = dot.Index + 2;
                prevId1 = prevId1 >= minIndex ? prevId1 : minIndex;
                prevId2 = prevId2 >= minIndex ? prevId2 : minIndex;
                nextId1 = nextId1 <= maxIndex ? nextId1 : maxIndex;
                nextId2 = nextId2 <= maxIndex ? nextId2 : maxIndex;
                var prevDots = new[] { prevId1, prevId2 }.Select(id => Dots[id]).ToList();
                var nextDots = new[] { nextId1, nextId2 }.Select(id => Dots[id]).ToList();
                var isIncisive = prevDots.Exists(d => dot.Y / d.Y > 1 + Const.SPG_IncisiveRatio) &&
                                 nextDots.Exists(d => dot.Y / d.Y > 1 + Const.SPG_IncisiveRatio);
                if (isIncisive) {
                    r.Add(dot);
                }
            }
            return r;
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

        public string GetDotsFeatureString()
        {
            var sb = new StringBuilder();
            foreach (var dot in Dots) {
                var fsb = new StringBuilder();
                foreach (var feature in dot.Features) {
                    fsb.Append($"{(int) feature.Name}-{feature.Ratio}");
                    if (feature != dot.Features.Last())
                        fsb.Append("|");
                }
                if (string.IsNullOrEmpty(fsb.ToString())) {
                    fsb.Append("0");
                }
                sb.Append(fsb);

                if (dot != Dots.Last())
                    sb.Append(",");
            }
            return sb.ToString();
        }

        #endregion

        #region Obsolete

        private const string _ObsoleteFeatureAlgorithmMessage = @"不再使用该方法计算得出特征值所在点的Y值. 现在的算法为: 计算每个峰值点都可能存在的特征值.";

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
            var rpsCoeff = FtCoeffDict[ftName];
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
