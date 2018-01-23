namespace PumpDiagnosticsSystem.HDataApp
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxPhyVibra = new System.Windows.Forms.TextBox();
            this.textBoxPhyNoVibra = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxPumpRun = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.loopCount = new System.Windows.Forms.Label();
            this.loopCur = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.PSNameLbl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(39, 56);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker1.TabIndex = 1;
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.CustomFormat = "yyyy-MM-dd HH:mm";
            this.dateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker2.Location = new System.Drawing.Point(39, 83);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(200, 21);
            this.dateTimePicker2.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(245, 70);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "开始模拟";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(450, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "机泵当前状态:";
            // 
            // textBoxPhyVibra
            // 
            this.textBoxPhyVibra.Location = new System.Drawing.Point(452, 189);
            this.textBoxPhyVibra.Multiline = true;
            this.textBoxPhyVibra.Name = "textBoxPhyVibra";
            this.textBoxPhyVibra.ReadOnly = true;
            this.textBoxPhyVibra.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPhyVibra.Size = new System.Drawing.Size(570, 267);
            this.textBoxPhyVibra.TabIndex = 8;
            this.textBoxPhyVibra.WordWrap = false;
            // 
            // textBoxPhyNoVibra
            // 
            this.textBoxPhyNoVibra.Location = new System.Drawing.Point(41, 189);
            this.textBoxPhyNoVibra.Multiline = true;
            this.textBoxPhyNoVibra.Name = "textBoxPhyNoVibra";
            this.textBoxPhyNoVibra.ReadOnly = true;
            this.textBoxPhyNoVibra.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPhyNoVibra.Size = new System.Drawing.Size(330, 273);
            this.textBoxPhyNoVibra.TabIndex = 9;
            this.textBoxPhyNoVibra.WordWrap = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(450, 161);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "图谱:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(39, 161);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "非振动:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(539, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "null";
            // 
            // textBoxPumpRun
            // 
            this.textBoxPumpRun.Location = new System.Drawing.Point(452, 36);
            this.textBoxPumpRun.Multiline = true;
            this.textBoxPumpRun.Name = "textBoxPumpRun";
            this.textBoxPumpRun.ReadOnly = true;
            this.textBoxPumpRun.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPumpRun.Size = new System.Drawing.Size(570, 98);
            this.textBoxPumpRun.TabIndex = 12;
            this.textBoxPumpRun.WordWrap = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(107, 127);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 12);
            this.label5.TabIndex = 13;
            this.label5.Text = "次, 当前循环:";
            // 
            // loopCount
            // 
            this.loopCount.AutoSize = true;
            this.loopCount.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.loopCount.ForeColor = System.Drawing.Color.Red;
            this.loopCount.Location = new System.Drawing.Point(86, 123);
            this.loopCount.Name = "loopCount";
            this.loopCount.Size = new System.Drawing.Size(18, 19);
            this.loopCount.TabIndex = 14;
            this.loopCount.Text = "1";
            // 
            // loopCur
            // 
            this.loopCur.AutoSize = true;
            this.loopCur.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.loopCur.ForeColor = System.Drawing.Color.Red;
            this.loopCur.Location = new System.Drawing.Point(196, 123);
            this.loopCur.Name = "loopCur";
            this.loopCur.Size = new System.Drawing.Size(18, 19);
            this.loopCur.TabIndex = 15;
            this.loopCur.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(43, 127);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 16;
            this.label6.Text = "共循环";
            // 
            // PSNameLbl
            // 
            this.PSNameLbl.AutoSize = true;
            this.PSNameLbl.Location = new System.Drawing.Point(39, 20);
            this.PSNameLbl.Name = "PSNameLbl";
            this.PSNameLbl.Size = new System.Drawing.Size(53, 12);
            this.PSNameLbl.TabIndex = 17;
            this.PSNameLbl.Text = "水厂名称";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1054, 490);
            this.Controls.Add(this.PSNameLbl);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.loopCur);
            this.Controls.Add(this.loopCount);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxPumpRun);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxPhyNoVibra);
            this.Controls.Add(this.textBoxPhyVibra);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dateTimePicker2);
            this.Controls.Add(this.dateTimePicker1);
            this.Name = "Form1";
            this.Text = "Redis历史数据写入";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxPhyVibra;
        private System.Windows.Forms.TextBox textBoxPhyNoVibra;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxPumpRun;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label loopCount;
        private System.Windows.Forms.Label loopCur;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label PSNameLbl;
    }
}

