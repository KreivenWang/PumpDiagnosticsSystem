using System;
using System.Collections.Generic;
using System.Linq;
using PumpDiagnosticsSystem.Core;
using PumpDiagnosticsSystem.Models;
using PumpDiagnosticsSystem.Util;
using RedisDemo.Redis;
using ServiceStack.Redis;

namespace PumpDiagnosticsSystem.Datas
{
    public class RedisToDataSource
    {

        public RedisToDataSource()
        {
            
        }

        /// <summary>
        /// 机泵实时数据
        /// </summary>
        public void UpdateRtData()
        {
            using (var redisClient = RedisManager.GetClient())
            {
                RuntimeRepo.RunningPumpGuids.Clear();

                try {
                    RuntimeRepo.RunningPumpGuids.AddRange(Repo.PumpGuids.Where(g => IsPumpRunningAndTdDataUpdated(redisClient, g)).ToList());
                } catch (ServiceStack.Redis.RedisException ex) {
                    Log.Error("实时数据更新失败! " + ex.Message);
                    return;
                }

                //更新每个ppsys的实时数据时间, 以所有传感器中最新的数据作为该时间
                RuntimeRepo.PumpSysTimeDict.Clear();
                foreach (var guid in RuntimeRepo.RunningPumpGuids) {
                    var ppsys = RuntimeRepo.PumpSysList.First(ps => GuidExt.IsSameGuid(guid, ps.Guid));
                    var maxTime = DateTime.MinValue;
                    foreach (var td in ppsys.Transducers) {
                        DateTime? time;
                        if (redisClient.TryGetTdPickTime(td, out time)) {
                            if (time > maxTime) {
                                maxTime = time.Value;
                            }
                        }
                    }
                    RuntimeRepo.PumpSysTimeDict.Add(guid, maxTime);
                }


                //构建要获取的实时数据的结构
                RuntimeRepo.RtData = new RtData();

                Log.Inform("--------------------------读取数据----------------------------");
                foreach (var item in RuntimeRepo.RtData.RedisKeyMap)
                {
                    var value = redisClient.GetValue(item.Key);
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (item.Value.Contains("$Ua") || item.Value.Contains("$Ub") || item.Value.Contains("$Uc"))
                        //     || item.Value.Contains("$P_In") || item.Value.Contains("$P_Out"))
                        {
                            RuntimeRepo.RtData.SpData[item.Value] = Convert.ToDouble(value) * 1000;
                        }
                        else
                        {
//                            if (item.Value.Contains("peed") && Convert.ToDouble(value) < 300)
//                            {
//                                var a = 1;
//                            }
                            RuntimeRepo.RtData.SpData[item.Value] = Convert.ToDouble(value);
                        }
                    }
                }
                //[vibration realtime data Example]:
                var specs = new List<Spectrum>();
                RuntimeRepo.SpecAnalyser.Specs.Clear();
                foreach (var graph in RuntimeRepo.RtData.Graphs) {
//                    var sgn = "{" + _waveDatasRedisKey[item.DeviceCode].ToUpper() + "}_" +
//                              (item.Type == "spectrum" ? "TimeWave" : "Spectrum");
                    var value = redisClient.GetValue(graph.Signal);
                    if (string.IsNullOrEmpty(value)) {
                        Log.Warn($"从Redis中获取不到 {graph.Signal} 对应的图谱");
                        continue;
                    }
                    graph.Time = DateTime.Parse(value.Split('|')[0].Replace(@"""", string.Empty));
                    var datas = value.Split('[')[1]
                        .Replace(@"\", string.Empty)
                        .Replace(@"""", string.Empty)
                        .Replace("]",string.Empty)
                        .Replace("}",string.Empty)
                        .Split(',')
                        .Select(double.Parse)
                        .ToList();
                    //不去掉第一个无效值0, 为了保证计算时索引和线保持一致
//                    if(datas[0] == 0D)
//                        datas.RemoveAt(0);
                    graph.UpdateData(datas.ToArray());
                    if (graph.Type == GraphType.Spectrum) {
                        var rpm = RuntimeRepo.GetRPM();
                        specs.Add(new Spectrum(graph.PPGuid, graph.SSGuid, rpm, graph.Data, graph.Pos));
                    }
                }
                RuntimeRepo.SpecAnalyser.UpdateSpecs(RuntimeRepo.RunningPumpGuids, specs);
            }
        }

        private bool IsPumpRunningAndTdDataUpdated(IRedisClient redisClient, Guid ppGuid)
        {
            //判断机泵是否在运行
            var isRun = redisClient.IsPumpRunning(ppGuid);
            Log.Inform(isRun
                ? $">>>>>>>>> 机组{ppGuid} 正在运行 >>>>>>>>"
                : $"--------- 机组{ppGuid} 未运行 ---------");
            if (!isRun)
                return false;

            //找出机泵下所有传感器，检查数据更新状态
            //如果都没更新， 那就不进行诊断
            //如果存在更新了数据的传感器，那就需要进行诊断
            var ppsys = RuntimeRepo.PumpSysList.First(ps => GuidExt.IsSameGuid(ppGuid, ps.Guid));
            var hasNewData = false;
            foreach (var td in ppsys.Transducers) {
                var isUpdated = redisClient.IsTdDataUpdated(td);
                hasNewData |= isUpdated;
            }
            if (!hasNewData)
                Log.Inform($"--------- 机组{ppGuid} 所有传感器未更新数值 ---------");
            return hasNewData;
        }
    }

    public static class RedisExtension
    {
        public static bool IsPumpRunning(this IRedisClient redisClient, Guid ppGuid)
        {
            var runSignal = redisClient.GetValue($"{{{ppGuid.ToString().ToUpper()}}}_Power");
            float isRun;
            if (float.TryParse(runSignal, out isRun)) {
                return (int)isRun != 0;
            }
            return false;
        }

        public static bool TryGetTdPickTime(this IRedisClient redisClient, BaseTransducer td, out DateTime? resultTime)
        {
            resultTime = null;
            var timeStr = string.Empty;
            switch (td.GetType().Name) {
                case nameof(SpeedTransducer):
                timeStr = redisClient.GetValue($"{{{td.Guid.ToFormatedString()}}}_SPEED_PICKDATE");
                break;
                case nameof(NonVibraTransducer):
                timeStr = redisClient.GetValue("IntouchUpdateTime");
                break;
                case nameof(VibraTransducer):
                timeStr = redisClient.GetValue("IntouchUpdateTime");
                break;
            }

            if (string.IsNullOrEmpty(timeStr)) {
                Log.Warn($"无法读取 {td.Guid} 的更新时间");
                return false;
            }

            timeStr = timeStr.Replace("\"", string.Empty);
            DateTime time;
            if (DateTime.TryParse(timeStr, out time)) {
                resultTime = time;
            }
            return true;
        }

        public static bool IsTdDataUpdated(this IRedisClient redisClient, BaseTransducer td)
        {
            DateTime? time;
            if (redisClient.TryGetTdPickTime(td, out time)) {

                //第一次进来是空，赋值, 并认为数值更新
                if (td.DataUpdateTime == null) { 
                    td.DataUpdateTime = time;
                    return true;
                }

                //时间相差大于1秒，则认为时间不同， 数值更新
                if (time - td.DataUpdateTime > TimeSpan.FromSeconds(1)) { 
                    td.DataUpdateTime = time;
                    Log.Inform($"{td.NameRemark}[传感器] {td.Code}已更新数据({td.DataUpdateTime})");
                    return true;
                }
            }
            return false;
        }
    }
}