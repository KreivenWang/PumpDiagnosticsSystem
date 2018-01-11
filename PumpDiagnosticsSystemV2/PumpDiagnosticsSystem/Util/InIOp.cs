using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PumpDiagnosticsSystem.Util
{
    public static class InIOp
    {
        private static string _iniPath;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        [DllImport("kernel32")]
        private static extern uint GetPrivateProfileString(
            string lpAppName, // points to section name
            string lpKeyName, // points to key name
            string lpDefault, // points to default string
            byte[] lpReturnedString, // points to destination buffer
            uint nSize, // size of destination buffer
            string lpFileName  // points to initialization filename

        );


        public static void Initialize(string iniPath)
        {
            _iniPath = iniPath;
        }

        /// <summary> 
        /// 写入INI文件 
        /// </summary> 
        /// <param name="Section">项目名称(如 [TypeName] )</param> 
        /// <param name="Key">键</param> 
        /// <param name="Value">值</param> 
        public static void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, _iniPath);
        }

        /// <summary> 
        /// 读出INI文件 
        /// </summary> 
        /// <param name="Section">项目名称(如 [TypeName] )</param> 
        /// <param name="Key">键</param> 
        public static string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(500);
            int i = GetPrivateProfileString(Section, Key, "", temp, 500, _iniPath);
            return temp.ToString();
        }

        /// <summary> 
        /// 验证文件是否存在 
        /// </summary> 
        /// <returns>布尔值</returns> 
        public static bool ExistINIFile()
        {
            return File.Exists(_iniPath);
        }

        /// <summary>
        /// 读取section
        /// </summary>
        /// <returns></returns>
        public static List<string> ReadSections()
        {
            List<string> result = new List<string>();
            byte[] buf = new byte[65536];
            uint len = GetPrivateProfileString(null, null, null, buf, (uint)buf.Length, _iniPath);
            int j = 0;
            for (int i = 0; i < len; i++)
                if (buf[i] == 0) {
                    result.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            return result;
        }


        /// <summary>
        /// 读取指定区域Keys列表。
        /// </summary>
        /// <param name="Section"></param>
        /// <returns></returns>
        public static List<string> ReadSingleSection(string Section)
        {
            List<string> result = new List<string>();
            byte[] buf = new byte[65536];
            uint lenf = GetPrivateProfileString(Section, null, null, buf, (uint)buf.Length, _iniPath);
            int j = 0;
            for (int i = 0; i < lenf; i++)
                if (buf[i] == 0) {
                    result.Add(Encoding.Default.GetString(buf, j, i - j));
                    j = i + 1;
                }
            return result;
        }
    }
}
