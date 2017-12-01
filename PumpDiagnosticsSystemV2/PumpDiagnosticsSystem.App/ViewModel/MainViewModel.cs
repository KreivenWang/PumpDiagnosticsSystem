using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using PumpDiagnosticsSystem.Business;
using PumpDiagnosticsSystem.Security;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.App.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private RegisterInfo _registerInfo;
        private bool _isAppRegistered => _registerInfo != null;

        #region IsSysRunning 属性

        private bool _backfield_IsSysRunning;

        public bool IsSysRunning
        {
            get { return _backfield_IsSysRunning; }
            set
            {
                _backfield_IsSysRunning = value;
                RaisePropertyChanged(() => IsSysRunning);
            }
        }

        #endregion

        #region RunText 属性

        private string _backfield_RunText;

        public string RunText
        {
            get { return _backfield_RunText; }
            set
            {
                _backfield_RunText = value;
                RaisePropertyChanged(() => RunText);
            }
        }

        #endregion

        #region MainText 属性
        private string _backfield_MainText;
        public string MainText
        {
            get
            {
                return _backfield_MainText;
            }
            set
            {
                _backfield_MainText = value;
                RaisePropertyChanged(() => MainText);
            }
        }
        #endregion

        #region RunModeText 属性
        private string _backfield_RunModeText;
        public string RunModeText
        {
            get { return _backfield_RunModeText; }
            set
            {
                _backfield_RunModeText = value;
                RaisePropertyChanged(() => RunModeText);
            }
        }
        #endregion


        #region SendReport 命令
        private ICommand _cmdSendReport;
        public ICommand SendReportCommand => _cmdSendReport ?? (_cmdSendReport = new RelayCommand(SendReport));

        private void SendReport()
        {
            MainController.ManualBuildUIReport();
            MessageBox.Show("诊断报告发送成功!", "生成结果", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Register 命令

        private ICommand _cmdRegister;
        public ICommand RegisterCommand => _cmdRegister ?? (_cmdRegister = new RelayCommand(Register));

        private void Register()
        {
            UpdateRegisterInfo();
            if (_isAppRegistered) {
                ShowRegistered();
            } else {
                ShowUnregistered();
            }
        }

        #endregion

        #region About 命令
        private ICommand _cmdAbout;
        public ICommand AboutCommand => _cmdAbout ?? (_cmdAbout = new RelayCommand(About));

        private void About()
        {
            MessageBox.Show($"{PubMembers.AppName}\r\ncopyright 2015-2017 上海航天动力科技工程有限公司", "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Run 命令

        private ICommand _cmdRun;
        public ICommand RunCommand => _cmdRun ?? (_cmdRun = new RelayCommand(Run, CanRun));

        private bool CanRun()
        {
            return !IsSysRunning;
        }

        private void Run()
        {
            UpdateRegisterInfo();
            if (!_isAppRegistered) {
                ShowUnregistered();
                return;
            }
            
            Task.Factory.StartNew(delegate
            {
                MainController.Initialze();
                MainController.RunProgramLoop();
            });
            IsSysRunning = true;
            RunText = "诊断运行中...";
        }

        #endregion

        #region OnLoaded 命令
        private ICommand _cmdOnLoaded;
        public ICommand OnLoadedCommand => _cmdOnLoaded ?? (_cmdOnLoaded = new RelayCommand(OnLoaded));

        private void OnLoaded()
        {
            //not expected effect
        }

        #endregion
        
        public ObservableCollection<PPSysView> PPSystems { get; } = new ObservableCollection<PPSysView>();

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            RunText = "启动诊断";
            MainText = "机泵健康诊断系统载入中...";
            PPSystems.Add(new PPSysView() { Name = "载入中", IsRunning = false });

            if (IsInDesignMode) {
                // Code runs in Blend --> create design time data.
                PPSystems.Clear();
                PPSystems.Add(new PPSysView() { Name = "13#机泵", IsRunning = false, Time = DateTime.Now.ToString("yy-MM-dd HH:mm") });
                PPSystems.Add(new PPSysView() { Name = "14#机泵", IsRunning = false, Time = DateTime.Now.ToString("yy-MM-dd HH:mm") });
                PPSystems.Add(new PPSysView() { Name = "15#机泵", IsRunning = false, Time = DateTime.Now.ToString("yy-MM-dd HH:mm") });
                PPSystems.Add(new PPSysView() { Name = "16#机泵", IsRunning = false, Time = DateTime.Now.ToString("yy-MM-dd HH:mm") });
                PPSystems.Add(new PPSysView() { Name = "17#机泵", IsRunning = false, Time = DateTime.Now.ToString("yy-MM-dd HH:mm") });
                IsSysRunning = true;
                RunModeText = "当前为：历史诊断模式";
            } else {
                //Check Registration
                UpdateRegisterInfo();
                RunModeText = Repo.IsHistoryDiagMode ? "当前为：历史诊断模式" : "当前为：实时诊断模式";

                RuntimeRepo.DataUpdated += RuntimeRepo_DataUpdated;
                Run();
            }
        }

        private void RuntimeRepo_DataUpdated()
        {
            MainText = $@"{Repo.PSInfo.PSName} - 机泵健康诊断系统";
            

            DispatcherHelper.CheckBeginInvokeOnUI(delegate
            {
                PPSystems.Clear();
                foreach (var ppsys in RuntimeRepo.PumpSysList.OrderBy(p=>p.Name)) {
                    var time = "数据无更新";
                    if (RuntimeRepo.PumpSysTimeDict.ContainsKey(ppsys.Guid)) {
                        time = RuntimeRepo.PumpSysTimeDict[ppsys.Guid]?.ToString("yy-MM-dd HH:mm");
                    }
                    PPSystems.Add(new PPSysView {
                        Name = ppsys.Name,
                        IsRunning = RuntimeRepo.RunningPumpGuids.Contains(ppsys.Guid),
                        Time = time
                    });
                }
            });
        }

        private void ShowUnregistered()
        {
            MessageBox.Show("软件未注册！请联系: 上海航天动力科技工程有限公司", "注册信息", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        private void ShowRegistered()
        {
            MessageBox.Show($"软件已注册。 注册信息：{_registerInfo.Remark}", "注册信息", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void UpdateRegisterInfo()
        {
            _registerInfo = RegisterOp.GetRegistInfo(MacAddress.GetByIPConfig());
        }
    }

    public class PPSysView
    {
        public string Name { get; set; }

        public bool IsRunning { get; set; }

        public string Time { get; set; }
    }
}