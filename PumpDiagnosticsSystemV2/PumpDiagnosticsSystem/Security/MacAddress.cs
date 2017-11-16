using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PumpDiagnosticsSystem.Security
{
    public static class MacAddress
    {
        ///<summary>
        /// 根据截取ipconfig /all命令的输出流获取网卡Mac
        ///</summary>
        ///<returns></returns>
        public static string GetByIPConfig()
        {
            List<string> macs = new List<string>();
            ProcessStartInfo startInfo = new ProcessStartInfo("ipconfig", "/all");
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            Process p = Process.Start(startInfo);
            //截取输出流
            StreamReader reader = p.StandardOutput;
            string line = reader.ReadLine();

            while (!reader.EndOfStream) {
                if (!string.IsNullOrEmpty(line)) {
                    line = line.Trim();

                    if (line.StartsWith("Physical Address") || line.StartsWith("物理地址")) {
                        macs.Add(line);
                    }
                }

                line = reader.ReadLine();
            }

            //等待程序执行完退出进程
            p.WaitForExit();
            p.Close();
            reader.Close();

            const string regexStr = @" : ([A-F0-9]{2}-){5}[A-F0-9]{2}";

            foreach (var mac in macs) {
                var matches = RegexMatch(mac, regexStr);
                if (!matches.Any())
                    continue;
                var match = matches[0];
                if (match != " : 00-00-00-00-00-00") {
                    return match.Remove(0, 3);
                }
            }

            return null;
        }

        public static List<string> RegexMatch(string expression, string regStr)
        {
            var rgx = new Regex(regStr, RegexOptions.Multiline);
            var matches = rgx.Matches(expression);
            return (from object match in matches select match.ToString()).ToList();
        }
    }
}
