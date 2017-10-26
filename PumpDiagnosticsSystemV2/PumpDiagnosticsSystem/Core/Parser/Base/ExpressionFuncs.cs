using System;
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
        }

        private double VibrationAmplitudeMax(double dataRowNo)
        {
            return PumpSystemFastQuery.GetBulkData((int)dataRowNo).Max();
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
                var graphData = RuntimeRepo.RtData.Graphs.FirstOrDefault(g => g.Number == (int)number)?.Data;
                if (graphData != null) {
                    //Log.Inform("当前函数SpectrumIntegration所需BulkDataTable记录的Id为" + number);

                    int startPoint = (int)Math.Round(startFrequence / frequenceInteval) - 1;
                    int endPoint = (int)Math.Round(endFrequence / frequenceInteval) + 1;

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
                LogToRtData("Start(Hz)", dotStart * frequenceInteval / 60);
                LogToRtData("End(Hz)", dotEnd * frequenceInteval / 60);
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
            return new[] { v1, v2, v3, v4 }.Max();
        }

        private double SpecFeature(double graphNumber, double feature, double speed, double frequenceInteval, double checkPeak)
        {
            var result = 0D;
            if (graphNumber > 0) {
                var graphData = RuntimeRepo.RtData.Graphs.FirstOrDefault(g => g.Number == (int)graphNumber)?.Data;
                if (graphData != null) {
                    var pointLeft = (int)Math.Round(feature * speed / frequenceInteval, MidpointRounding.AwayFromZero);
                    var pointRight = pointLeft + 1;

                    var gdtLeft = graphData[pointLeft];
                    var gdtRight = graphData[pointRight];

                    var higherGdtPoint = gdtRight > gdtLeft ? pointRight : pointLeft;

                    var dotStart = higherGdtPoint - 1;
                    var dotEnd = higherGdtPoint + 1;

                    var gdtStart = graphData[dotStart];
                    var gdtMiddle = graphData[higherGdtPoint];
                    var gdtEnd = graphData[dotEnd];

                    result = gdtStart + gdtMiddle + gdtEnd;

                    //需要检查波峰
                    if (checkPeak != 0D) {
                        //中间值必须为波峰
                        if (!(gdtMiddle > gdtStart && gdtMiddle > gdtEnd)) {
                            result = -1;
                        }
                    }

                    var dotPreview = $"{gdtStart}, {gdtMiddle}, {gdtEnd}";
                    LogToRtData(nameof(SpecFeature), result);
                    LogToRtData("Start(Hz)", dotStart * frequenceInteval / 60);
                    LogToRtData("End(Hz)", dotEnd * frequenceInteval / 60);
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
    }
}
