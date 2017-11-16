using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PumpDiagnosticsSystem.Dbs;
using PumpDiagnosticsSystem.Security;

namespace PumpDiagnosticsSystem.Reg
{
    public partial class RegForm : Form
    {
        private const string ClientMac = "ClientMac";
        private readonly string _connStr;
        private readonly string _localMac;

        public RegForm()
        {
            InitializeComponent();

            _connStr = AccessOp.ConnStr;

            _localMac = MacAddress.GetByIPConfig();
            macTxtBox.Text = _localMac;
            Shown += Form1_Loaded;
        }

        private void Form1_Loaded(object sender, EventArgs e)
        {
            RegisterInfo regInfo;
            try {
                regInfo =  RegisterOp.GetRegistInfo(_localMac);
            } catch (Exception ex) {
                label1.Text = "数据库连接错误!" + ex.Message;
                btnReg.Enabled = false;
                return;
            }

            if (regInfo == null) {
                label1.Text = "软件未注册";
                label1.ForeColor = Color.Red;
                remarkTxtBox.Text = "请输入水厂信息";
                remarkTxtBox.Focus();
            } else {
                label1.Text = "软件已注册";
                label1.ForeColor = Color.Green;
                macTxtBox.Text = regInfo.Mac;
                remarkTxtBox.Text = regInfo.Remark;
                btnReg.Enabled = false;
            }
        }

        public DataTable LoadTable(string tableName)
        {
            var conn = new OleDbConnection(_connStr);
            try {
                var ds = new DataSet();
                conn.Open();
                var query = "Select * from " + tableName;
                var adapter = new OleDbDataAdapter(query, conn);
                adapter.Fill(ds, tableName);
                return ds.Tables[tableName];
            } catch (Exception ex) {
                MessageBox.Show($"无法读取Access表：{tableName},{ex.Message}");
                Application.Exit();
                return null;
            } finally {
                conn.Close();
            }
        }

        private void btnReg_Click(object sender, EventArgs e)
        {
            var conn = new OleDbConnection(_connStr);
            try {
                conn.Open();
                var query = $"INSERT INTO {ClientMac} (mac, remark) VALUES ('{macTxtBox.Text}', '{remarkTxtBox.Text}')";
                var cmd = new OleDbCommand(query, conn);
                var result = cmd.ExecuteNonQuery();
                if (result == 1) {
                    label1.Text = "注册成功";
                    label1.ForeColor = Color.DodgerBlue;
                    btnReg.Enabled = false;
                } else {
                    label1.Text = "注册失败";
                    label1.ForeColor = Color.Red;
                }
            } catch (Exception ex) {
                MessageBox.Show($"无法读取Access表：{ClientMac},{ex.Message}");
                Application.Exit();
            } finally {
                conn.Close();
            }
        }
    }
}
