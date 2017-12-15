using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace PumpDiagnosticsSystem.Util
{
    public class ConsoleToLogHelper : TextWriter
    {
        private static readonly string _directory = Directory.GetCurrentDirectory() + "\\Logs\\";
        private static string _lastWrite = string.Empty;
        /// <summary>
        /// 当在派生类中重写时，返回用来写输出的 <see cref="T:System.Text.Encoding"/>。
        /// </summary>
        /// <returns>
        /// 用来写入输出的 Encoding。
        /// </returns>
        public override Encoding Encoding => Encoding.UTF8;

        public static void Initialize()
        {
            Console.SetOut(new ConsoleToLogHelper());
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
        }

        /// <summary>
        /// 压力测试：连续循环写入100000条文本数据，程序正常
        /// </summary>
        /// <param name="text"></param>
        private static void AppendToTodayFile(string text)
        {
            //如果和上一行内容相同, 则不重复写入
            if (text == _lastWrite) return;

            var filePath = _directory + DateTime.Now.Date.ToString("yyyy-MM-dd") + ".log";
            var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            fs.Position = fs.Length;//追加文本到文件末尾
            var sw = new StreamWriter(fs);
            sw.Write(text);
            sw.Flush();
            sw.Close();
            fs.Close();
            _lastWrite = text;
        }

        #region Overrides of TextWriter

        /// <summary>
        /// 将字符串写入文本流。
        /// </summary>
        /// <param name="value">要写入的字符串。</param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed.</exception><exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
        public override void Write(string value)
        {
            base.Write(value);
            AppendToTodayFile(value);
        }

        #endregion

        /// <summary>
        /// 将后跟行结束符的字符串写入文本流。
        /// </summary>
        /// <param name="value">要写入的字符串。如果 <paramref name="value"/> 为 null，则仅写入行结束字符。</param><exception cref="T:System.ObjectDisposedException"><see cref="T:System.IO.TextWriter"/> 是关闭的。</exception><exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
        public override void WriteLine(string value)
        {
            if (!string.IsNullOrWhiteSpace(value)) {
                base.WriteLine(value);
                AppendToTodayFile($"{DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss] ")}{value}{Environment.NewLine}");
            } else {
                WriteLine();
            }
        }

        /// <summary>
        /// 将行结束符写入文本流。
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException"><see cref="T:System.IO.TextWriter"/> 是关闭的。</exception><exception cref="T:System.IO.IOException">发生 I/O 错误。</exception>
        public override void WriteLine()
        {
            base.WriteLine();
            AppendToTodayFile(Environment.NewLine);
        }
    }

    public static class Log
    {
        public static bool ShowLogInform { get; } = bool.Parse(ConfigurationManager.AppSettings["ShowLogInform"]);

        public static void Inform(string text, bool forceLog = false)
        {
            if(forceLog)
                Console.WriteLine(text);
            else if (ShowLogInform)
                Console.WriteLine(text);
        }

        public static void Inform(bool forceLog = false)
        {
            Inform(string.Empty, forceLog);
        }

        public static void Warn(string text)
        {
            Console.WriteLine("【警告】" + text);
        }

        public static void Error(string text)
        {
            Console.WriteLine("【错误！】" + text);
        }
    }
}