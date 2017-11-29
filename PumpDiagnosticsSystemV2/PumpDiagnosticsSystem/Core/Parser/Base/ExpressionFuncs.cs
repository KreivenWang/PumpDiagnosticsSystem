using System;
using System.Collections.Generic;
using System.Linq;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Core.Parser.Base
{
    public partial class ExpressionParser
    {
        public event Action<FuncParseResult> FuncParsed;

        private void LogToRtData(string funcName, double value)
        {
            FuncParsed?.Invoke(new FuncParseResult(funcName, value));
        }

        private void RegisterFuncs()
        {
            DefineFun(nameof(VibrationAmplitudeMax), VibrationAmplitudeMax, true);
            DefineFun(nameof(SpectrumIntegration), SpectrumIntegration, true);
            DefineFun(nameof(FMax), FMax, true);
            DefineFun(nameof(MaxOf4), MaxOf4, true);
            DefineFun(nameof(SpecFeature), SpecFeature, true);
            DefineFun(nameof(CheckSyntony), CheckSyntony, true);
            DefineFun(nameof(FeatureInNoise), FeatureInNoise, true);
            DefineFun(nameof(TestStringFunc), TestStringFunc);
        }

        private double VibrationAmplitudeMax(double dataRowNo)
        {
            return PumpSystemFastQuery.GetBulkData((int) dataRowNo).Max();
        }

        private double SpectrumIntegration(double number, double startFrequence, double endFrequence,
            double frequenceInteval)
        {
            var result = 0D;
            var dotCount = 0;
            var dotPreview = string.Empty;
            var dotStart = -1;
            var dotEnd = -1;
            if (number > 0) {

                //小于0的起始频率按0来算
                if (startFrequence < 0)
                    startFrequence = 0;

                //            if (!frequenceInteval.HasValue)
                //                frequenceInteval = 1.25 * 60;
                //List<double> datas = GetData(number);
                //            var datas = PumpSystemFastQuery.GetBulkData((int)number);
                var graphData = RuntimeRepo.RtData.Graphs.FirstOrDefault(g => g.Number == (int) number)?.Data;
                if (graphData != null) {
                    //Log.Inform("当前函数SpectrumIntegration所需BulkDataTable记录的Id为" + number);

                    int startPoint = (int) Math.Round(startFrequence/frequenceInteval) - 1;
                    int endPoint = (int) Math.Round(endFrequence/frequenceInteval) + 1;

                    int point = 0;
                    if (startPoint > point)
                        point = startPoint;
                    if (endPoint > graphData.Count)
                        endPoint = graphData.Count - 1;

                    //从第二个点开始算
                    point++;

                    dotStart = point;
                    dotEnd = endPoint;
                    for (; point <= endPoint; point++) {
                        result += graphData[point];
                        dotCount++;
                        dotPreview += Math.Round(graphData[point], 6) + " ";
                    }
                } else {
                    Log.Warn($"SpectrumIntegration函数：编号为{number}的图谱未找到");
                    result = -1;
                }
            }
            LogToRtData(nameof(SpectrumIntegration), result);
            if (dotCount > 0) {
                LogToRtData("Start(Hz)", dotStart*frequenceInteval/60);
                LogToRtData("End(Hz)", dotEnd*frequenceInteval/60);
                if (dotCount <= 10) {
                    LogToRtData(dotPreview, dotCount);
                } else {
                    LogToRtData("Count", dotCount);
                }
            }

            return result;
        }

        /// <summary>
        /// 根据转速,计算出振动数据采集频率上限
        /// </summary>
        /// <param name="rpm">转速</param>
        /// <returns></returns>
        private double FMax(double rpm)
        {
            var result = Spectrum.FMax(rpm);
            LogToRtData(nameof(FMax), result);
            return result;
        }

        private double MaxOf4(double v1, double v2, double v3, double v4)
        {
            return new[] {v1, v2, v3, v4}.Max();
        }

        private double SpecFeature(double graphNumber, double feature, double speed, double frequenceInteval,
            double checkPeak)
        {
            if (speed <= 0D) {
                Log.Error("SpecFeature函数传入的转速为0.");
                return -1;
            }
            var result = 0D;
            if (graphNumber > 0) {
                var graphData = RuntimeRepo.RtData.Graphs.FirstOrDefault(g => g.Number == (int) graphNumber)?.Data;
                if (graphData != null) {
                    var pointLeft = (int) Math.Round(feature*speed/frequenceInteval, MidpointRounding.AwayFromZero);
                    var pointRight = pointLeft + 1;

                    var gdtLeft = graphData[pointLeft];
                    var gdtRight = graphData[pointRight];

                    var higherGdtPoint = gdtRight > gdtLeft ? pointRight : pointLeft;

                    var dotStart = higherGdtPoint - 1;
                    var dotEnd = higherGdtPoint + 1;

                    var gdtStart = graphData[dotStart];
                    var gdtMiddle = graphData[higherGdtPoint];
                    var gdtEnd = graphData[dotEnd];

                    //原来用的是3点之和， 现在只用波峰就行了
                    //result = gdtStart + gdtMiddle + gdtEnd;
                    result = gdtMiddle;

                    //需要检查波峰
                    if (checkPeak != 0D) {
                        //中间值必须为波峰
                        if (!(gdtMiddle > gdtStart && gdtMiddle > gdtEnd)) {
                            result = -1;
                        }
                    }

                    var dotPreview = $"{gdtStart}, {gdtMiddle}, {gdtEnd}";
                    LogToRtData(nameof(SpecFeature), result);
                    LogToRtData("Start(Hz)", dotStart*frequenceInteval/60);
                    LogToRtData("End(Hz)", dotEnd*frequenceInteval/60);
                    LogToRtData(dotPreview, 3);

                } else {
                    Log.Warn($"SpecFeature：编号为{graphNumber}的图谱未找到");
                    result = -1;
                }
            }
            return result;
        }

        /// <summary>
        /// 判断共振
        /// </summary>
        /// <param name="ratio"></param>
        /// <param name="driver">从左到右:水泵非驱动端到电机非驱动端为0,1,2,3</param>
        /// <returns></returns>
        private double CheckSyntony(double ratio, double driver)
        {
            var result = 0D;

            var specs =
                RuntimeRepo.SpecAnalyser.BrPoses_Specs_Dict[RuntimeRepo.DiagnosingPumpSys.Guid][(int) driver].ToList();
            specs.RemoveAll(s => s == null);

            if (!specs.Any()) {
                Log.Warn($"CheckSyntony: 轴承{driver}中XYZ均不包含图谱");
                return result;
            }

            var maxVibraValue = specs.Max(s => s.MVDot.Y);
            var maxVibraSpec = specs.First(s => s.MVDot.Y == maxVibraValue);
            var maxVibraFeatures = maxVibraSpec.MVDot.Features;

            //主频点中存在特征值超标
            if (maxVibraFeatures.Exists(ft => maxVibraSpec.High(ft))) {

                //且 大于ratio倍的 至少一个 其他2个方向 的振值
                foreach (var spec in specs) {
                    LogToRtData(spec.Pos.DirectionPos.ToString(), spec.MVDot.Y);
                    if (maxVibraSpec != spec) {
                        if (maxVibraValue >= spec.MVDot.Y*ratio) {
                            result = 1D;
                        }
                    }
                }
            }

            //如果没有共振的话，记录所有的当前值，供参考分析
            if (result == 0D) {
                foreach (var spec in specs) {
                    LogToRtData(spec.Pos.DirectionPos.ToString(), spec.MVDot.Y);
                }
            }
            return result;
        }

//        private double FeatureInNoise(double graphNumber, double noiseRatio, double ftNames, double sidePeakGroup)
//        {
//            var result = 0D;
////            var fts = PubFuncs.ParseEnumFlags<Spectrum.FtName>((int)ftNames);

//            var fts = (Spectrum.FtName) ftNames;
//            var spGroup = (Spectrum.SidePeakGroupType) sidePeakGroup;

//            var spec = RuntimeRepo.SpecAnalyser.Specs.FirstOrDefault(s => s.GraphNumber == (int) graphNumber);
//            if (spec == null) {
//                Log.Warn($"SpectrumIntegration函数：编号为{graphNumber}的图谱未找到");
//                result = -1;
//                return result;
//            }

//            //存在底脚区域
//            var specNoiseIndexes = spec.FindNoises(noiseRatio);
//            if (!specNoiseIndexes.Any()) {
//                LogToRtData("底脚数", 0);
//                return result;
//            }

//            for (int i = 1; i <= specNoiseIndexes.Count; i++) {
//                var noiseBegin = specNoiseIndexes[i].Item1;
//                var noiseEnd = specNoiseIndexes[i].Item2;
//                LogToRtData("底脚", i);
//                LogToRtData("起始点", noiseBegin);
//                LogToRtData("结束点", noiseEnd);

//                var noiseDots =
//                    spec.Dots.FindAll(d => d.Index >= noiseBegin && d.Index <= noiseEnd);

//                //区域中存在有特征频率的点
//                if (noiseDots.Exists(d => d.Features.Exists(f => fts.HasFlag(f.Name)))) {
//                    result = 1;
//                }

//                //区域中存在有边频带峰群的点集
//                //存在至少一个边频带峰群: 它的所有点都在噪音区内
//                var sidePeakGroups = spec.FindSidePeaksGroups();
//                if (sidePeakGroups.FindAll(g => g.Type == spGroup)
//                    .Exists(g => g.SidePeaks.IsSubsetOf(noiseDots) && noiseDots.Contains(g.MainPeak))) {
//                    result = 1;
//                }
//            }

//            return result;
//        }

        /// <summary>
        /// 底脚噪声中， 主峰边频带和谐波的判断
        /// </summary>
        /// <param name="graphNumber">频谱编号</param>
        /// <param name="freqRegions">频段划分</param>
        /// <param name="footerGrades">底脚分档</param>
        /// <param name="autoFooterGrade">底脚自动档</param>
        /// <param name="featureCount"></param>
        /// <param name="sidePeakGroup">主峰边频带</param>
        /// <param name="nxFeature">谐波</param>
        /// <returns></returns>
        private double FeatureInNoise(double graphNumber, double freqRegions, double footerGrades, double autoFooterGrade,
            double featureCount, double sidePeakGroup, double nxFeature)
        {
            var result = 0D;

            var spec = RuntimeRepo.SpecAnalyser.Specs.FirstOrDefault(s => s.GraphNumber == (int) graphNumber);
            if (spec == null) {
                Log.Warn($"SpectrumIntegration函数：编号为{graphNumber}的图谱未找到");
                result = -1;
                return result;
            }

            var fqRegionTypes = (Spectrum.FreqRegionType) freqRegions;
            const Spectrum.FreqRegionType allFqRegionTypes = Spectrum.FreqRegionType.Low |
                                                             Spectrum.FreqRegionType.Middle |
                                                             Spectrum.FreqRegionType.High;

            //2. 根据所在频段计算满足条件的点的个数, 再出结果
            var judgeResult = new Func<List<Spectrum.Dot>, double>(dots =>
            {
                //先设置所有频段所有点
                var availableRegions = spec.FreqRegions;
                var availableDots = dots;

                //若非所有频段, 则对点进行筛选
                if (fqRegionTypes != allFqRegionTypes) {
                    availableRegions = availableRegions.FindAll(r => fqRegionTypes.HasFlag(r.Type));
                    availableDots =
                        dots.FindAll(
                            dot => availableRegions.Exists(r => dot.X > r.BeginFrequence && dot.X < r.EndFrequence));
                }

                if (availableDots.Any()) {
                    var count = 0;

                    //不为-1即为要判断的
                    if (sidePeakGroup > -1D) {
                        var spGroupTypes = (Spectrum.SidePeakGroupType) sidePeakGroup;
                        //区域中存在有边频带峰群的点集
                        //存在至少 count 个边频带峰群: 它的所有点都在噪音区内
                        var availableSidePeakGroups = spec.FindSidePeaksGroups().FindAll(g => g.Type == spGroupTypes);
                        availableSidePeakGroups = availableSidePeakGroups.FindAll(
                                g => g.SidePeaks.IsSubsetOf(availableDots) && availableDots.Contains(g.MainPeak));

                        foreach (var spg in availableSidePeakGroups) {
                            LogToRtData($"{spg.Type.ToString()}主峰坐标:[{spg.MainPeak.X},{spg.MainPeak.Y}],编号",spg.MainPeak.Index);
                            foreach (var sp in spg.SidePeaks) {
                                LogToRtData($"边频带:[{sp.X},{sp.Y}],编号", sp.Index);
                            }
                        }

                        count = availableSidePeakGroups.Count;
                    }

                    if (nxFeature > -1D) {
                        var ftNames = (Spectrum.FtName) nxFeature;
                        //区域中存在至少 count 个谐波点
                        var featureDots = availableDots.FindAll(d => d.Features.Exists(f => f.Name == ftNames));

                        foreach (var fdot in featureDots) {
                            LogToRtData(
                                $"{ftNames.ToString()}{fdot.Features.First(f => f.Name == ftNames).Ratio.ToString()}X谐波坐标:[{fdot.X},{fdot.Y}],编号",
                                fdot.Index);
                        }

                        count = featureDots.Count;
                    }

                    if (count >= featureCount) {
                        return 1D;
                    }

                } else {
                    LogToRtData($"非所有点都在所需频段{fqRegionTypes}内", 0);
                }
                return 0D;
            });

            //1. 判断底脚
            var footerGradeTypes = (Spectrum.FooterGradeType)footerGrades;
            if (footerGradeTypes == 0D) {
                //做些不需要底脚的判断
                result = judgeResult(spec.Dots);
            } else {

                foreach (
                    var footerGradeType in
                        PubFuncs.ParseEnumFlags<Spectrum.FooterGradeType>(15).Where(fg => footerGradeTypes.HasFlag(fg))) {

                    //存在底脚区域
                    var specNoiseIndexes = spec.FindNoises(footerGradeType, (int) autoFooterGrade);

                    if (!specNoiseIndexes.Any()) {
                        LogToRtData("底脚数", 0);
                        //return result;
                        continue;
                    }

                    //对每个底脚区域
                    for (int i = 0; i < specNoiseIndexes.Count; i++) {

                        var noiseBegin = specNoiseIndexes[i].Item1;
                        var noiseEnd = specNoiseIndexes[i].Item2;
                        LogToRtData("底脚", i + 1);
                        LogToRtData("起始点", noiseBegin);
                        LogToRtData("结束点", noiseEnd);

                        var noiseDots = spec.Dots.FindAll(d => d.Index >= noiseBegin && d.Index <= noiseEnd);

                        result = judgeResult(noiseDots);
                    }
                }
            }

            return result;
        }

        private double TestStringFunc(string str1, double val1)
        {
            return str1.Length + (int) val1;
        }
    }
}
