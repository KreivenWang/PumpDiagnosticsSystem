using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace PumpDiagnosticsSystem.App
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static NotifyIcon NotifyIcon { get; set; }

        public App()
        {
            GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();
        }

        #region Overrides of Application

        /// <summary>
        /// 引发 <see cref="E:System.Windows.Application.Exit"/> 事件。
        /// </summary>
        /// <param name="e">包含事件数据的 <see cref="T:System.Windows.ExitEventArgs"/> 。</param>
        protected override void OnExit(ExitEventArgs e)
        {
            NotifyIcon.Visible = false;
            base.OnExit(e);
        }

        #endregion
    }
}
