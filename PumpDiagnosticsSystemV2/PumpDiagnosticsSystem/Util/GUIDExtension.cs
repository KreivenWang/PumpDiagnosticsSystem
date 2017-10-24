using System;

namespace PumpDiagnosticsSystem.Util
{
    public static class GuidExt
    {
        public static string ToFormatedString(this Guid guid)
        {
            return guid.ToString().ToUpper();
        }

        public static bool IsSameGuid(Guid g1, Guid g2)
        {
            return g1.Equals(g2);
        }

        public static bool IsSameGuid(Guid g1, string g2Str)
        {
            return IsSameGuid(g1.ToString(), g2Str);
        }

        public static bool IsSameGuid(string g1Str, string g2Str)
        {
            //其实在2个str都确保格式正确的情况下，  比较2个gstr.ToUpper()就行了，guid.ToString()默认是小写的字母
            Guid g1, g2;
            if (Guid.TryParse(g1Str, out g1) &&
                Guid.TryParse(g2Str, out g2)) {
                return g1 == g2;
            }
            Log.Error($"GUID字符串格式不正确： {g1Str} 或 {g2Str}");
            return false;
        }
    }
}