using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpDiagnosticsSystem.HDataApp
{
    class RemoteFileHelper
    {

        /// <summary>
        /// 使用方法：
        /// if (Connect("192.168.1.48", "用户名", "密码"))   
        /// {
        ///     File.Copy(@"\\192.168.1.48\共享目录\test.txt",   @"e:\\test.txt",   true);   
        /// }
        /// </summary>
        /// <param name="remoteHost"></param>
        /// <param name="userName"></param>
        /// <param name="passWord"></param>
        /// <returns></returns>
        public static bool Connect(string remoteHost, string userName, string passWord)
        {
            bool flag = true;
            Process proc = new Process {
                StartInfo = {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            try {
                proc.Start();
                string command = $@"net use {remoteHost} {passWord} /user:{userName}>NUL";
                proc.StandardInput.WriteLine(command);
                command = "exit";
                proc.StandardInput.WriteLine(command);
                while (proc.HasExited == false) {
                    proc.WaitForExit(1000);
                }
                string errormsg = proc.StandardError.ReadToEnd();
                if (errormsg != "")
                    flag = false;
                proc.StandardError.Close();
            } catch (Exception ex) {
                flag = false;
            } finally {
                proc.Close();
                proc.Dispose();
            }
            return flag;
        }
    }
}
