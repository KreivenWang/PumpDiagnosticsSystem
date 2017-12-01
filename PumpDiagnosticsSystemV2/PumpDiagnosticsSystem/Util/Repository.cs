using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using PumpDiagnosticsSystem.Core;
using PumpDiagnosticsSystem.Core.Constructor;
using PumpDiagnosticsSystem.Core.Parser;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Models;

namespace PumpDiagnosticsSystem.Util
{
    /// <summary>
    /// 公用数据仓库，用于保存和读取静态数据，不要在其他地方修改里面的值
    /// </summary>
    public static class Repo
    {
        /// <summary>
        /// 各种文本的分隔符
        /// </summary>
        public const string Separator = ", ";

        public static PumpStationInfo PSInfo { get; private set; }

        public static Guid[] PumpGuids { get; private set; }

        /// <summary>
        /// 传感器配置
        /// </summary>
        public static IList<SENSOR> SensorList { get; private set; }

        /// <summary>
        /// 信号量配置
        /// </summary>
        public static IList<PHYDEFNOVIBRA> PhyDefNoVibra { get; private set; }


        /// <summary>
        /// 故障项表
        /// </summary>
        public static List<FaultItem> FaultItems { get; } = new List<FaultItem>();

        /// <summary>
        /// 判据表
        /// </summary>
        public static List<Criterion> Criteria { get; } = new List<Criterion>();

        /// <summary>
        /// 推断树
        /// </summary>
        public static List<InferComboItem> InferCombos { get; } = new List<InferComboItem>();

        /// <summary>
        /// 判据中用到的常量表
        /// </summary>
        public static Dictionary<string, double> Consts { get; } = new Dictionary<string, double>();

        /// <summary>
        /// 频谱特征值的报警值
        /// </summary>
        public static Dictionary<string, double> SpecFtLimits { get; } = new Dictionary<string, double>();

        /// <summary>
        /// 是否为历史诊断模式
        /// </summary>
        public static bool IsHistoryDiagMode { get; } = bool.Parse(ConfigurationManager.AppSettings["IsHistoryDiagMode"]);

        /// <summary>
        /// 每天自动生成报告的时间
        /// </summary>
        public static DateTime ReportBuildTime { get; } = DateTime.Parse(ConfigurationManager.AppSettings["ReportBuildTime"]);

        public static void Initialize()
        {
            //获取水厂信息
            PSInfo = DataDetailsOp.GetPumpStationInfo();
            

            //获取信号量配置
            PhyDefNoVibra = SqlUtil.GetPhydefNoVibraList();


            //获取传感器配置
            SensorList = SqlUtil.GetSensorList();


            PumpGuids = PhyDefNoVibra.Select(p => p.PPGUID).Distinct().ToArray();
            

            //构建判据
            LogicConstructor.ConstructRepo();


            #region 常量部分

            try {
                foreach (DataRow row in PumpSysLib.TableConst.Rows) {
                    Consts.Add(row["ConstName"].ToString(), double.Parse(row[PSInfo.PSCode].ToString()));
                }
            } catch (ArgumentException) {
                Log.Error($"常量表中找不到 泵站名称：{PSInfo.PSCode ?? "未配置"} 对应的列");
            }

            #endregion

            #region 频谱相关

            #region 特征值报警值部分

            foreach (DataRow row in PumpSysLib.TableSpectrumAlarm.Rows) {
                var ftNameStr = row["FtName"].ToString();
                var lmtExp = row["FtAlarm"].ToString();

                //替换报警值表达式中的常量
                foreach (var @const in Repo.Consts) {
                    if (lmtExp.Contains(@const.Key))
                        lmtExp = lmtExp.Replace(@const.Key, @const.Value.ToString());
                }

                //计算表达式得出报警值, 计算结果保留6位小数
                var lmt = Math.Round(EasyParser.Parse(lmtExp), 6);

                SpecFtLimits.Add(ftNameStr, lmt);
            }

            #endregion

            SpecConst.Init();

            #endregion
        }

        /// <summary>
        /// 通过完整的EventMode找出其所对应的故障项
        /// </summary>
        /// <param name="fullEventModeStr"></param>
        /// <returns></returns>
        public static FaultItem FindFaultItem(string fullEventModeStr)
        {
            if (!FaultItems.Any())
                Log.Error("故障项未初始化！");
            return FaultItems.FirstOrDefault(fi => $"{Map.TypeToEnum.First(t=>t.Value == fi.CompType).Key}_{fi.EventMode}" == fullEventModeStr);
        }

        /// <summary>
        /// 映射/对应关系
        /// </summary>
        public static class Map
        {
            /// <summary>
            /// 组件类型 至 信号量要对应的默认变量
            /// </summary>
            public static Dictionary<CompType, string> CompDfVar => new Dictionary<CompType, string> {
                {CompType.Td_P, "@P"},
                {CompType.Td_T, "@T"},
                {CompType.Td_S, "@Speed"},
                {CompType.Td_V, "@OverAll"},

            };

            /// <summary>
            /// 信号量类型标志 至 变送器类型
            /// </summary>
            public static Dictionary<string, CompType> SignalToTdType => new Dictionary<string, CompType> {
                {"$T_", CompType.Td_T},
                {"$P_", CompType.Td_P},
                {"$Speed_", CompType.Td_S},
                {"$V_", CompType.Td_V},
                {"$Ua_", CompType.PV},
                {"$Ub_", CompType.PV},
                {"$Uc_", CompType.PV},
                {"$Ia_", CompType.PA},
                {"$Ib_", CompType.PA},
                {"$Ic_", CompType.PA},
                {"$Frequence_", CompType.PF}

            };

            /// <summary>
            /// Access组件库中组件类型 至 枚举组件类型
            /// </summary>
            public static Dictionary<string, CompType> TypeToEnum => new Dictionary<string, CompType> {
                {"WaterPump", CompType.Pump},
                {"Motor", CompType.Motor},
                {"Coupler", CompType.Coupler},
                {"PressureTransducer", CompType.Td_P},
                {"VibrationTransducer", CompType.Td_V},
                {"TTransducer", CompType.Td_T},
                {"SpeedTransducer", CompType.Td_S},
                {"AmMeter", CompType.PA},
                {"VoltMeter", CompType.PV},
                {"FrequenceMeter", CompType.PF}
            };

            /// <summary>
            /// 图谱的@变量名 至 Redis中的图谱变量名
            /// </summary>
            public static Dictionary<GraphType, string> GraphToRedisGraph => new Dictionary<GraphType, string> {
                {GraphType.Phase, "Phase"},
                {GraphType.Spectrum, "Spec"},
                {GraphType.TimeWave, "WaveTime"}
            };
        }

        public static class SpecConst
        {
            /// <summary>
            /// 判断Y方向零位的系数
            /// </summary>
            public static double ZeroRatio { get; private set; } = 1.5D;

            /// <summary>
            /// 频率宽度(Hz)
            /// </summary>
            public static double BandWidth { get; private set; } = 1000D;

            /// <summary>
            /// 报警值,暂时没用到,还是用的access的const表中的#MAX_A_VIBRATOIN
            /// </summary>
            public static double AlarmValue { get; private set; } = 11.42D;

            /// <summary>
            /// 噪音区域分割点列表,需要乘上转速, 目前只用了第一个, 用来判定噪音的最小宽度
            /// </summary>
            public static List<double> NoiseRegionPartitions { get; private set; } = new List<double> {
                0D,
                20D,
                50D
            };

            /// <summary>
            /// 低频与中频段的界限(转速倍数)
            /// </summary>
            public static double FreqRegion_LowToMiddle { get; private set; } = 40D;

            /// <summary>
            /// 中频与高频段的界限(比例)
            /// </summary>
            public static double FreqRegion_MiddleToHigh { get; private set; } = 0.5D;


            #region 底脚相关

            /// <summary>
            /// 噪声点判定(Y方向) - 底数
            /// </summary>
            public static double NoiseAlarmJudge_Base { get; private set; } = 1.6D;

            /// <summary>
            /// 噪声点判定(Y方向) - 幂 - 下限
            /// </summary>
            public static int NoiseJudge_Pow_Min { get; private set; } = 0;

            /// <summary>
            /// 噪声点判定(Y方向) - 幂 - 上限
            /// </summary>
            public static int NoiseJudge_Pow_Max { get; private set; } = 9;

            /// <summary>
            /// 判断为噪声点的报警值百分比(Y方向)
            /// </summary>
            public static double NoiseAlarmPercent { get; private set; } = 0.01D;

            #endregion

            /// <summary>
            /// 判断为噪声的最小宽度百分比(X方向)
            /// </summary>
            public static double NoiseMinWidthPercent { get; private set; } = 1D / 3D;

            /// <summary>
            /// 判断为特征点频率的容差
            /// </summary>
            public static double FtJudgeTolerance { get; private set; } = 0.000625D;//V1: 0.1找到了4个特征一样的点 

            public static void Init()
            {
                foreach (DataRow row in PumpSysLib.TableSpectrumConst.Rows) {
                    var name = row["ConstName"].ToString();
                    if (name == nameof(ZeroRatio))
                        ZeroRatio = double.Parse(row[PSInfo.PSCode].ToString());

                    else if (name == nameof(BandWidth))
                        BandWidth = double.Parse(row[PSInfo.PSCode].ToString());

                    else if (name == nameof(AlarmValue))
                        AlarmValue = double.Parse(row[PSInfo.PSCode].ToString());

                    else if (name == nameof(NoiseRegionPartitions))
                        NoiseRegionPartitions = row[PSInfo.PSCode].ToString().Split(',')
                            .Select(double.Parse)
                            .ToList();

                    else if (name == nameof(FreqRegion_LowToMiddle))
                        FreqRegion_LowToMiddle = double.Parse(row[PSInfo.PSCode].ToString());

                    else if (name == nameof(FreqRegion_MiddleToHigh))
                        FreqRegion_MiddleToHigh = double.Parse(row[PSInfo.PSCode].ToString());

                    else if (name == nameof(NoiseAlarmJudge_Base))
                        NoiseAlarmJudge_Base = double.Parse(row[PSInfo.PSCode].ToString());

                    else if (name == nameof(NoiseJudge_Pow_Min))
                        NoiseJudge_Pow_Min = int.Parse(row[PSInfo.PSCode].ToString());

                    else if (name == nameof(NoiseJudge_Pow_Max))
                        NoiseJudge_Pow_Max = int.Parse(row[PSInfo.PSCode].ToString());

                    else if (name == nameof(NoiseAlarmPercent))
                        NoiseAlarmPercent = double.Parse(row[PSInfo.PSCode].ToString());

                    else if (name == nameof(NoiseMinWidthPercent))
                        NoiseMinWidthPercent = double.Parse(row[PSInfo.PSCode].ToString());

                    else if (name == nameof(FtJudgeTolerance))
                        FtJudgeTolerance = double.Parse(row[PSInfo.PSCode].ToString());
                    else {
                        Log.Error($"频谱常量表中找不到 泵站名称：{PSInfo.PSCode ?? "未配置"} 对应的列{name}");
                    }
                }
            }
        }
    }
}
