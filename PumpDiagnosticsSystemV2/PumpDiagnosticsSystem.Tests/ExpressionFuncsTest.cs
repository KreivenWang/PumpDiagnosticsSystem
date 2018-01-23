using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PumpDiagnosticsSystem.Core;
using PumpDiagnosticsSystem.Core.Parser.Base;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Tests
{
    [TestClass]
    public class ExpressionFuncsTest
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphNum">单个图谱测试都写1就行了</param>
        /// <param name="rpm">每分钟转速</param>
        /// <param name="ppGuid">机泵Guid</param>
        /// <param name="ssGuid">传感器Guid</param>
        /// <param name="pos">传感器的位置(参考app.ini中的设置)</param>
        /// <param name="specFilePath">图谱的文件路径</param>
        public void LoadSpectrum(int graphNum, double rpm, Guid ppGuid, Guid ssGuid, string pos, string specFilePath)
        {
            var graph = new Graph {
                PPGuid = ppGuid,
                SSGuid = ssGuid,
//                Signal = PubFuncs.FormatGraphSignal(sensor.SSGUID.ToString(), GraphType.Spectrum),
                Number = graphNum,
                Pos = pos,
                Type = GraphType.Spectrum
            };

            var fs = new FileStream(specFilePath, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs);
            var value = sr.ReadToEnd();
            sr.Close();
            fs.Close();

            //mock datetime
            value = value.Insert(0, $"{DateTime.Now}|");

            //var value = redisClient.GetValue(graph.Signal);
            //            if (string.IsNullOrEmpty(value)) {
            //                Log.Warn($"从Redis中获取不到 {graph.Signal} 对应的图谱");
            //                continue;
            //            }

            Spectrum spec = null;

            graph.Time = DateTime.Parse(value.Split('|')[0].Replace(@"""", string.Empty));
            var datas = value.Split('[')[1]
                .Replace(@"\", string.Empty)
                .Replace(@"""", string.Empty)
                .Replace("]", string.Empty)
                .Replace("}", string.Empty)
                .Split(',')
                .Select(double.Parse)
                .ToList();
            //不去掉第一个无效值0, 为了保证计算时索引和线保持一致
            //                    if(datas[0] == 0D)
            //                        datas.RemoveAt(0);
            graph.UpdateData(datas.ToArray());
            if (graph.Type == GraphType.Spectrum) {
                var bandWidthInfo = value.Split('|')[1].Split(',')[0].Split(':');
                if (bandWidthInfo[0].Replace(" ", string.Empty) == "{FMax") {
                    graph.BandWidth = double.Parse(bandWidthInfo[1].Replace(" ", string.Empty));
                }

                //var rpm = RuntimeRepo.GetRPM();
                spec = new Spectrum(
                    ppGuid: graph.PPGuid,
                    ssGuid: graph.SSGuid,
                    graphNum: graph.Number,
                    rpm: rpm,
                    bandWidth: graph.BandWidth,
                    data: graph.Data,
                    tdPos: graph.Pos);

                //spec的dots 存到 graph
                //graph.FeatureStr = spec.GetDotsFeatureString();
            }

            RuntimeRepo.RtData = new RtData();
            RuntimeRepo.SpecAnalyser.UpdateSpecs(new List<Guid> { ppGuid }, new List<Spectrum> { spec });
            
        }

        /// <summary>
        /// 水泵汽蚀的噪声判据测试
        /// //20171225, 根据实验汽蚀模拟的结果来调整算法
        /// </summary>
        [TestMethod]
        public void WaterPump_Cavitation_Condition_NoiseTest()
        {
            Repo.Initialize();
            var parser = new ExpressionParser();

            var ppguid = Guid.Parse("A67C1178-3B96-412F-BBEF-045B926F9999");
            var ssguid = Guid.Parse("05EE47E6-50FD-4171-8C68-D4A874F99007");
            var pos = "PUMPIN_X";
            var graphNum = 1;

            var testAction = new Func<double, string, bool>((rpm, specData) =>
            {
                LoadSpectrum(graphNum, rpm, ppguid, ssguid, pos, specData);

                var footerGradeCoeff = 1;
                //判据279-282
                var result = parser.FeatureInNoise(
                    graphNumber: graphNum,
                    freqRegions: 6,
                    footerGrades: 8,
                    autoFooterGrade: footerGradeCoeff,
                    featureCount: 0,
                    sidePeakGroup: -1,
                    nxFeature: -1);
                return result == 1;
            });

            //转速从监测界面的历史频谱中读出
            Assert.IsTrue(testAction(2036.556D, @"\\192.168.0.9\XMData\shiyan\2017\12\25\1_7_15.20.00_spec.txt"));
            Assert.IsTrue(testAction(2033.216D, @"\\192.168.0.9\XMData\shiyan\2017\12\25\1_7_15.25.00_spec.txt"));
            Assert.IsTrue(testAction(2012.364D, @"\\192.168.0.9\XMData\shiyan\2017\12\25\1_7_15.29.00_spec.txt"));

            Assert.IsFalse(testAction(2327.669D, @"\\192.168.0.9\XMData\shiyan\2017\12\18\1_7_13.55.00_spec.txt"));
            Assert.IsFalse(testAction(2939.246D, @"\\192.168.0.9\XMData\shiyan\2017\12\18\1_7_15.30.00_spec.txt"));

        }
    }
}
