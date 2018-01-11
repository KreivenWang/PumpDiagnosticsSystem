using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using PumpDiagnosticsSystem.Datas;
using PumpDiagnosticsSystem.Util;

namespace PumpDiagnosticsSystem.HDataApp
{
    public partial class Form1 : Form
    {
        private DateTime d1 = DateTime.MinValue;
        private DateTime d2 = DateTime.MaxValue;
        private static readonly Dictionary<string, IEnumerable<string>> _phyDefNoVibra = new Dictionary<string, IEnumerable<string>>();
        private static readonly Dictionary<string, string> _pumpRun = new Dictionary<string, string>();
        private static IList<SENSOR> _sensorList;
        private readonly StringBuilder SBphynovibra = new StringBuilder();
        private readonly StringBuilder SBphyvibra = new StringBuilder();
        private readonly StringBuilder SBpumprun = new StringBuilder();
        private static readonly int _inv = Convert.ToInt32(ConfigurationManager.AppSettings["Inv"]) * 1000;
        private static readonly int _loopCount = Convert.ToInt32(ConfigurationManager.AppSettings["Loop"]);
        private int _loopCur = 0;


        public Form1()
        {
            InitializeComponent();
            GlobalRepo.Initialize();
            //            RemoteFileHelper.Connect("")

            //获取信号量配置
            var phyDefNoVibra = SqlUtil.GetPhydefNoVibraList().Where(p => p.PSGUID == GlobalRepo.PSInfo.PSGuid && p.PSGUID != p.PPGUID).ToList();
            var groupByPPGUID = from q in phyDefNoVibra
                                group q by q.PPGUID into g
                                select new { ppGuid = g.Key.ToString(), PDNVCODEs = g.Select(p => p.PDNVCODE) };


            foreach (var item in groupByPPGUID) {
                _phyDefNoVibra.Add(item.ppGuid, item.PDNVCODEs);
            }


            foreach (var item in phyDefNoVibra) {
                if (item.PDNVTYPE == "1") {
                    _pumpRun.Add(item.PPGUID.ToString(), item.PDNVCODE);
                }
            }

            //获取传感器配置
            var ppguids = phyDefNoVibra.Select(s => s.PPGUID).Distinct();
            _sensorList = SqlUtil.GetSensorList().Where(p => ppguids.Contains(p.PPGUID)).ToList();

            using (new SharedTool("Administrator", "123.net", "192.168.0.9")) {
                string selectPath = @"\\192.168.0.9\XMData";
                MessageBox.Show("共享文件夹权限已获取");
            }

            this.loopCount.Text = _loopCount.ToString();
            button1_Click(null, null);
        }

        private void BatchData()
        {
            var max = (d2 - d1).TotalMinutes;
            for (int i = 0; i <= max; i = i + 5) {
                SBphynovibra.Clear();
                SBphyvibra.Clear();
                SBpumprun.Clear();
                SqlUtil.GetDatFromSqlToRedis(d1.AddMinutes(i).ToString("yyyy-MM-dd HH:mm:ss"), _sensorList, _phyDefNoVibra, _pumpRun, SBphynovibra, SBphyvibra, SBpumprun);
                SetText(SBphynovibra.ToString(), SBphyvibra.ToString(), SBpumprun.ToString(), d1.AddMinutes(i).ToString("yyyy-MM-dd HH:mm:ss"));
                Thread.Sleep(_inv);
//                if (i >= max) {
//                    i = 0;
//                }
            }
        }

        delegate void SetTextCallBack(string Stringphynovibra, string Stringphyvibra, string Stringpumprun, string text2);
        private void SetText(string Stringphynovibra, string Stringphyvibra, string Stringpumprun, string text2)
        {
            if (this.textBoxPhyVibra.InvokeRequired) {
                SetTextCallBack stcb = new SetTextCallBack(SetText);
                if (this.IsHandleCreated && this.IsDisposed == false) {
                    this.Invoke(stcb, new object[] { Stringphynovibra, Stringphyvibra, Stringpumprun, text2 });
                }
            } else {
                this.textBoxPhyNoVibra.Text = Stringphynovibra;
                this.textBoxPhyVibra.Text = Stringphyvibra;
                this.textBoxPumpRun.Text = Stringpumprun;
                this.label3.Text = text2;
            }
        }

        delegate void SetLoopCallBack(string text);
        private void SetLoop(string text)
        {
            if (this.textBoxPhyVibra.InvokeRequired) {
                SetLoopCallBack stcb = new SetLoopCallBack(SetLoop);
                if (this.IsHandleCreated && this.IsDisposed == false) {
                    this.Invoke(stcb, text);
                }
            } else {
                this.loopCur.Text = text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;
            //d1 = Convert.ToDateTime(dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm"));// .AddHours(-dateTimePicker1.Value.Hour).AddMinutes(-dateTimePicker1.Value.Minute).AddSeconds(-dateTimePicker1.Value.Second) ;
            //d2 = Convert.ToDateTime(dateTimePicker2.Value.ToString("yyyy-MM-dd HH:mm"));
            d1 = Convert.ToDateTime(dateTimePicker1.Value.ToString(ConfigurationManager.AppSettings["BeginTime"]));// .AddHours(-dateTimePicker1.Value.Hour).AddMinutes(-dateTimePicker1.Value.Minute).AddSeconds(-dateTimePicker1.Value.Second) ;
            d2 = Convert.ToDateTime(dateTimePicker2.Value.ToString(ConfigurationManager.AppSettings["EndTime"]));
            dateTimePicker1.Value = d1;
            dateTimePicker2.Value = d2;
            dateTimePicker1.Enabled = false;
            dateTimePicker2.Enabled = false;
            Task.Factory.StartNew(() =>
            {
                for (; _loopCur <= _loopCount; _loopCur++) {
                    SetLoop(_loopCur.ToString());
                    BatchData();
                }

                SetLoop("运行已结束");
            });
        }
    }
}
