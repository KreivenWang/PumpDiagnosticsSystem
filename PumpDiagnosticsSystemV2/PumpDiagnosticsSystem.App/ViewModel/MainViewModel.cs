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

        #region IsSysRunning ����

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

        #region RunText ����

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

        #region MainText ����
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

        #region RunModeText ����
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


        #region SendReport ����
        private ICommand _cmdSendReport;
        public ICommand SendReportCommand => _cmdSendReport ?? (_cmdSendReport = new RelayCommand(SendReport));

        private void SendReport()
        {
            MainController.ManualBuildUIReport();
            MessageBox.Show("��ϱ��淢�ͳɹ�!", "���ɽ��", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Register ����

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

        #region About ����
        private ICommand _cmdAbout;
        public ICommand AboutCommand => _cmdAbout ?? (_cmdAbout = new RelayCommand(About));

        private void About()
        {
            MessageBox.Show($"{PubMembers.AppName}\r\ncopyright 2015-2017 �Ϻ����춯���Ƽ��������޹�˾", "����", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Run ����

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
            RunText = "���������...";
        }

        #endregion

        #region OnLoaded ����
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
            RunText = "�������";
            MainText = "���ý������ϵͳ������...";
            PPSystems.Add(new PPSysView() { Name = "������", IsRunning = false });

            if (IsInDesignMode) {
                // Code runs in Blend --> create design time data.
                PPSystems.Clear();
                PPSystems.Add(new PPSysView() { Name = "13#����", IsRunning = false, Time = DateTime.Now.ToString("yy-MM-dd HH:mm") });
                PPSystems.Add(new PPSysView() { Name = "14#����", IsRunning = false, Time = DateTime.Now.ToString("yy-MM-dd HH:mm") });
                PPSystems.Add(new PPSysView() { Name = "15#����", IsRunning = false, Time = DateTime.Now.ToString("yy-MM-dd HH:mm") });
                PPSystems.Add(new PPSysView() { Name = "16#����", IsRunning = false, Time = DateTime.Now.ToString("yy-MM-dd HH:mm") });
                PPSystems.Add(new PPSysView() { Name = "17#����", IsRunning = false, Time = DateTime.Now.ToString("yy-MM-dd HH:mm") });
                IsSysRunning = true;
                RunModeText = "��ǰΪ����ʷ���ģʽ";
            } else {
                //Check Registration
                UpdateRegisterInfo();
                RunModeText = Repo.IsHistoryDiagMode ? "��ǰΪ����ʷ���ģʽ" : "��ǰΪ��ʵʱ���ģʽ";

                RuntimeRepo.DataUpdated += RuntimeRepo_DataUpdated;
                Run();
            }
        }

        private void RuntimeRepo_DataUpdated()
        {
            MainText = $@"{Repo.PSInfo.PSName} - ���ý������ϵͳ";
            

            DispatcherHelper.CheckBeginInvokeOnUI(delegate
            {
                PPSystems.Clear();
                foreach (var ppsys in RuntimeRepo.PumpSysList.OrderBy(p=>p.Name)) {
                    var time = "�����޸���";
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
            MessageBox.Show("���δע�ᣡ����ϵ: �Ϻ����춯���Ƽ��������޹�˾", "ע����Ϣ", MessageBoxButton.OK, MessageBoxImage.Question);
        }

        private void ShowRegistered()
        {
            MessageBox.Show($"�����ע�ᡣ ע����Ϣ��{_registerInfo.Remark}", "ע����Ϣ", MessageBoxButton.OK,
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