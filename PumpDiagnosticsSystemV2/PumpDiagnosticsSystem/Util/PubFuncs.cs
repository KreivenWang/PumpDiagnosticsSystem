﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using PumpDiagnosticsSystem.Datas;
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

        public static string FindTdPosFromSignal(string signal)
        {
            //var tdpos = SysConstants.SENSORSETTING[$"{sensor.LOCATION}_{sensor.DIRECTION}"];
            //return tdpos;

            foreach (var pos in SysConstants.AllTdPoses) {
                if (signal.Contains(pos + "_")) {
                    return pos;
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

        /// <summary>
        /// 找到数组中连续不断的值的始末对组成的数组
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tolerance">容差, 向后延伸t个点的都算做连续</param>
        /// <returns></returns>
        public static List<Tuple<int, int>> FindContinues(List<int> data, int tolerance)
        {
            var result = new List<Tuple<int, int>>();
            for (int i = 0; i < data.Count; i++) {
                var tolCount = tolerance;
                var start = data[i];
                var forward = start + 1;
                var j = i + 1;
                for (; j < data.Count; j++) {
                    if (data[j] == forward) {
                        forward++;
                        tolCount = tolerance;//恢复容差数量
                    } else {
                        if (tolCount > 0) {
                            tolCount--;
                            forward++;
                            j--;//避免j继续增加
                        } else {
                            break;
                        }
                    }
                }
                if (j != i + 1) {
                    if (!result.Exists(t => t.Item2 == data[j - 1])) {
                        result.Add(new Tuple<int, int>(start, data[j - 1]));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 列表是否包含指定索引
        /// </summary>
        public static bool ContainsIndex<T>(this IList<T> list, int index)
        {
            return index >= 0 && index < list.Count;
        }

        /// <summary>
        /// 转化枚举值为枚举列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static List<T> ParseEnumFlags<T>(int flags)
        {
            return typeof(T).GetEnumNames()
                .Where(n => (flags & (int)Enum.Parse(typeof(T), n)) != 0)
                .Select(n => (T)Enum.Parse(typeof(T), n))
                .ToList();
        }

        /// <summary>
        /// 根据正则表达式获取匹配项
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="regStr"></param>
        /// <returns></returns>
        public static List<string> RegexMatch(string expression, string regStr)
        {
            var rgx = new Regex(regStr, RegexOptions.Multiline);
            var matches = rgx.Matches(expression);
            return (from object match in matches select match.ToString()).ToList();
        }

        public static double ToRound(this double value, int digits = 5)
        {
            return Math.Round(value, digits);
        }
    }
}
