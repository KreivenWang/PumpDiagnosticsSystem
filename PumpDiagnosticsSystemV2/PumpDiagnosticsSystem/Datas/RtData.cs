using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Models.DbEntities;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.Datas
{
    /// <summary>
    /// RealtimeData
    /// </summary>
    public class RtData
    {
        public Dictionary<string, string> RedisKeyMap { get; } = new Dictionary<string, string>();

        /// <summary>
        /// 单点数据
        /// </summary>
        public Dictionary<string, double> SpData { get; } = new Dictionary<string, double>();

        /// <summary>
        /// 图谱数据
        /// </summary>
        public List<Graph> Graphs { get; } = new List<Graph>();


//        public double Speed { get; set; }

        /// <summary>
        /// 根据机泵运行情况构建的要获取的实时数据的结构
        /// </summary>
        public RtData()
        {

            #region 构建【单点数据】的结构，并设置如何映射到Redis中

            foreach (var ppGuid in RuntimeRepo.RunningPumpGuids) {

                foreach (var phy in SysConstants.SENSORSETTING.Values) { //add to SENSORSETTING
                    var vibraSignal = $"${phy}_{ppGuid}";
                    var phaseSignal = VibraTransducer.ConvertSignalToPhaseSignal(vibraSignal);
                    SpData.Add(vibraSignal, default(double));
                    SpData.Add(phaseSignal, default(double));
                    var sensor = Repo.SensorList.FirstOrDefault(p =>
                        SysConstants.SENSORSETTING[p.LOCATION + "_" + p.DIRECTION] == phy &&
                        ppGuid == p.PPGUID);
                    if (sensor != null) {
                        var keyVibra = $"{{{sensor.SSGUID}}}_{SysConstants.VibraFields.Overall}".ToUpper();
                        RedisKeyMap.Add(keyVibra, vibraSignal);
                        var keyPhase = $"{{{sensor.SSGUID}}}_{SysConstants.VibraFields.V1Phase}".ToUpper();
                        RedisKeyMap.Add(keyPhase, phaseSignal);
                    } else {
                        Log.Warn($"实时数据构建：振动传感器未找到: {phy} ppguid: {ppGuid} (将导致相关判据无法解析)");
                    }
                }

                foreach (var phynv in SysConstants.PHYDEF_NONVIBRA) {
                    var noVibraSignal = $"${phynv}_{ppGuid}";
                    SpData.Add(noVibraSignal, default(double));
                    var sensor = Repo.PhyDefNoVibra.FirstOrDefault(p => p.REMARK == phynv && ppGuid == p.PPGUID);
                    if (sensor != null) {
                        var keyNoVibra = $"{{{ppGuid}}}_{sensor.PDNVCODE}".ToUpper();
                        RedisKeyMap.Add(keyNoVibra, noVibraSignal);
                    } else {
                        Log.Warn($"实时数据构建：非振动传感器未找到: {phynv} ppguid: {ppGuid} (将导致相关判据无法解析)");
                    }
                }

                var speedSignal = SpeedTransducer.FormatTdSpeedSignal(ppGuid.ToString());
                SpData.Add(speedSignal, default(double));
                var keySpeed = $"{{{ppGuid}}}_{SysConstants.VibraFields.Speed}".ToUpper();
                RedisKeyMap.Add(keySpeed, speedSignal);
            }

            #endregion


            #region 构建【振动图谱数据】的结构

            int graphNumber = 1;
            foreach (var pumpGuid in RuntimeRepo.RunningPumpGuids) {
                foreach (var sensorSetting in SysConstants.SENSORSETTING) {
                    var sensor =
                        Repo.SensorList.FirstOrDefault(
                            p => p.LOCATION + "_" + p.DIRECTION == sensorSetting.Key && pumpGuid == p.PPGUID);
                    var pos = sensorSetting.Value;
                    Debug.Assert(pos != null, "sensorSetting 传感器位置无法解析");
                    if (sensor == null)
                        continue;
                    Graphs.Add(new Graph {
                        PPGuid = pumpGuid,
                        SSGuid = sensor.SSGUID,
                        Signal = PubFuncs.FormatGraphSignal(sensor.SSGUID.ToString(), GraphType.Spectrum),
                        Number = graphNumber++,
                        Pos = pos,
                        Type = GraphType.Spectrum
                    });

                    Graphs.Add(new Graph {
                        PPGuid = pumpGuid,
                        SSGuid = sensor.SSGUID,
                        Signal = PubFuncs.FormatGraphSignal(sensor.SSGUID.ToString(), GraphType.TimeWave),
                        Number = graphNumber++,
                        Pos = pos,
                        Type = GraphType.TimeWave
                    });
                }
            }

            #endregion

        }

        //private TdPos? FindPosFromSignal(string str)
        //{
        //    foreach (var name in Enum.GetNames(typeof(TdPos))) {
        //        if (str.Contains(name))
        //            return (TdPos)Enum.Parse(typeof (TdPos), name);
        //    }
        //    return null;
        //}

        public double? FindSignalValue(string signal)
        {
            if (SpData.ContainsKey(signal)) {
                return SpData[signal];
            }
            var graph = Graphs.FirstOrDefault(g => g.Signal == signal);
            if (graph != null) {
                return graph.Number;
            }
            return null;
        }
    }
}
