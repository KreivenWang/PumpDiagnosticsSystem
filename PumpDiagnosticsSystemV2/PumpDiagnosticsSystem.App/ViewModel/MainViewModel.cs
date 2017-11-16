using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using PumpDiagnosticsSystem.Business;
using PumpDiagnosticsSystem.Security;

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
            MessageBox.Show("copyright 2017 上海航天动力科技工程有限公司", "关于", MessageBoxButton.OK, MessageBoxImage.Information);
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
            RunText = "运行中...";
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

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            RunText = "启动";

            //Check Registration
            UpdateRegisterInfo();

            if (IsInDesignMode) {
                // Code runs in Blend --> create design time data.
            } else {
                // Code runs "for real"
            }
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
}