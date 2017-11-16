using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using PumpDiagnosticsSystem.App.ViewModel;

namespace PumpDiagnosticsSystem.App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            //系统托盘
            SystemTrayParameter pars = new SystemTrayParameter("logoicon.ico", PubMembers.AppName, "", 0, notifyIcon_MouseDoubleClick);
            SystemTray.SetSystemTray(pars, GetList());
            //            WinCommon.WinBaseSet(this);
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        #region 系统托盘

        //托盘右键菜单集合
        private List<SystemTrayMenu> GetList()
        {
            List<SystemTrayMenu> ls = new List<SystemTrayMenu>();
            ls.Add(new SystemTrayMenu() { Txt = "打开主面板", Icon = "", Click = mainWin_Click });
            ls.Add(new SystemTrayMenu() { Txt = "退出", Icon = "", Click = exit_Click });
            return ls;
        }
        //双击事件
        void notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
//            this._notifyIcon.Visible = false;
        }

        #region 托盘右键菜单
        //打开主面板
        void mainWin_Click(object sender, EventArgs e)
        {
            this.Show();
            //this.notifyIcon.Visible = false;
        }

        //退出
        void exit_Click(object sender, EventArgs e)
        {
            Close();
            System.Windows.Application.Current.Shutdown();
        }
        #endregion
        #endregion
    }
}
