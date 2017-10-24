using System.Collections.Generic;
using System.Linq;

namespace PumpDiagnosticsSystem.Util
{
    public static class ListExtensions
    {
        /// <summary>
        /// 检查不重复后添加item到列表
        /// </summary>
        public static void AddSingle<T>(this List<T> list, T item)
        {
            if (!list.Contains(item)) {
                list.Add(item);
            }
        }

        /// <summary>
        /// 检查不重复后添加items到列表
        /// </summary>
        public static void AddRangeWithRepeatCheck<T>(this List<T> list, List<T> items)
        {
            foreach (var item in items) {
                list.AddSingle(item);
            }
        }

        /// <summary>
        /// 比较2个列表是否等同(元素相同, 顺序相同)
        /// </summary>
        public static bool IsEqualTo<T>(this List<T> listA, List<T> listB)
        {
            if (listA.Count != listB.Count)
                return false;
            var sortedListA = listA.ToList();
            sortedListA.Sort();
            var sortedListB = listB.ToList();
            sortedListB.Sort();
            for (int i = 0; i < sortedListA.Count; i++) {
                if (!Equals(sortedListA[i], sortedListB[i])) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 比较2个列表是否等同(AB列表相互包含, 顺序可以不同)
        /// </summary>
        public static bool IsElementsSameAs<T>(this List<T> listA, List<T> listB)
        {
            return listA.Except(listB).ToList().Count == 0;
        }
    }
}
