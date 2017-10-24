using System;
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

        
    }
}
