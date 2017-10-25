using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using PumpDiagnosticsSystem.Models;

namespace PumpDiagnosticsSystem.Util
{
    /// <summary>
    /// 公用方法
    /// </summary>
    public static class PubFuncs
    {
        public static CompType? ParseSignalType(string signal)
        {
            CompType? type = null;
            foreach (var map in Repo.Map.SignalToTdType) {
                if (signal.StartsWith(map.Key)) {
                    type = map.Value;
                    break;
                }
            }
            return type;
        }

        public static TdPos? FindTdPosFromSignal(string signal)
        {
            var etype = typeof(TdPos);
            foreach (var pos in Enum.GetNames(etype)) {
                if (signal.Contains(pos + "_")) {
                    return (TdPos) Enum.Parse(etype, pos);
                }
            }
            return null;
        }

        /// <summary>
        /// 获取对象的深复制副本
        /// </summary>
        /// <typeparam name="T">所要深复制的对象类型</typeparam>
        /// <param name="obj">所要深复制的对象引用</param>
        /// <returns>深复制副本</returns>
        public static T DeepClone<T>(this T obj)
        {
            using (Stream stream = new MemoryStream()) {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)serializer.Deserialize(stream);
            }
        }

        public static string FormatGraphSignal(string ssGuid, GraphType gType)
        {
            var redisGraphType = Repo.Map.GraphToRedisGraph[gType];
            return $"{{{ssGuid.ToUpper()}}}_{redisGraphType}";
        }

        /// <summary>
        /// 找到数组中连续不断的值的始末对组成的数组
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<Tuple<int, int>> FindContinues(List<int> data)
        {
            var result = new List<Tuple<int, int>>();
            for (int i = 0; i < data.Count; i++) {
                var start = data[i];
                var forward = start + 1;
                var j = i + 1;
                for (; j < data.Count; j++) {
                    if (data[j] != forward) {
                        break;
                    }
                    forward++;
                }
                if (j != i + 1) {
                    if (!result.Exists(t => t.Item2 == data[j - 1]))
                        result.Add(new Tuple<int, int>(start, data[j - 1]));
                }
            }
            return result;
        }
    }
}
