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
            //��ʵ��2��str��ȷ����ʽ��ȷ������£�  �Ƚ�2��gstr.ToUpper()�����ˣ�guid.ToString()Ĭ����Сд����ĸ
            Guid g1, g2;
            if (Guid.TryParse(g1Str, out g1) &&
                Guid.TryParse(g2Str, out g2)) {
                return g1 == g2;
            }
            Log.Error($"GUID�ַ�����ʽ����ȷ�� {g1Str} �� {g2Str}");
            return false;
        }
    }
}