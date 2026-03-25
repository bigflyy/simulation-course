namespace DiscreteRV
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new();

            this.lblP1 = new Label();
            this.lblP2 = new Label();
            this.lblP3 = new Label();
            this.lblP4 = new Label();
            this.lblP5Caption = new Label();
            this.lblP5Value = new Label();
            this.lblN = new Label();
            this.nudP1 = new NumericUpDown();
            this.nudP2 = new NumericUpDown();
            this.nudP3 = new NumericUpDown();
            this.nudP4 = new NumericUpDown();
            this.nudN = new NumericUpDown();
            this.btnRun = new Button();
            this.btnRunAll = new Button();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.txtResults = new TextBox();

            ((System.ComponentModel.ISupportInitialize)this.nudP1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudP2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudP3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudP4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.nudN).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.chart1).BeginInit();
            this.SuspendLayout();

            // lblP1
            this.lblP1.Text = "p1:";
            this.lblP1.Location = new System.Drawing.Point(12, 15);
            this.lblP1.AutoSize = true;

            // nudP1
            this.nudP1.Location = new System.Drawing.Point(40, 12);
            this.nudP1.Size = new System.Drawing.Size(70, 23);
            this.nudP1.DecimalPlaces = 2;
            this.nudP1.Increment = 0.05m;
            this.nudP1.Maximum = 1.00m;
            this.nudP1.Minimum = 0.00m;
            this.nudP1.Value = 0.25m;
            this.nudP1.ValueChanged += new EventHandler(this.NudP_ValueChanged);

            // lblP2
            this.lblP2.Text = "p2:";
            this.lblP2.Location = new System.Drawing.Point(120, 15);
            this.lblP2.AutoSize = true;

            // nudP2
            this.nudP2.Location = new System.Drawing.Point(148, 12);
            this.nudP2.Size = new System.Drawing.Size(70, 23);
            this.nudP2.DecimalPlaces = 2;
            this.nudP2.Increment = 0.05m;
            this.nudP2.Maximum = 1.00m;
            this.nudP2.Minimum = 0.00m;
            this.nudP2.Value = 0.15m;
            this.nudP2.ValueChanged += new EventHandler(this.NudP_ValueChanged);

            // lblP3
            this.lblP3.Text = "p3:";
            this.lblP3.Location = new System.Drawing.Point(228, 15);
            this.lblP3.AutoSize = true;

            // nudP3
            this.nudP3.Location = new System.Drawing.Point(256, 12);
            this.nudP3.Size = new System.Drawing.Size(70, 23);
            this.nudP3.DecimalPlaces = 2;
            this.nudP3.Increment = 0.05m;
            this.nudP3.Maximum = 1.00m;
            this.nudP3.Minimum = 0.00m;
            this.nudP3.Value = 0.20m;
            this.nudP3.ValueChanged += new EventHandler(this.NudP_ValueChanged);

            // lblP4
            this.lblP4.Text = "p4:";
            this.lblP4.Location = new System.Drawing.Point(336, 15);
            this.lblP4.AutoSize = true;

            // nudP4
            this.nudP4.Location = new System.Drawing.Point(364, 12);
            this.nudP4.Size = new System.Drawing.Size(70, 23);
            this.nudP4.DecimalPlaces = 2;
            this.nudP4.Increment = 0.05m;
            this.nudP4.Maximum = 1.00m;
            this.nudP4.Minimum = 0.00m;
            this.nudP4.Value = 0.20m;
            this.nudP4.ValueChanged += new EventHandler(this.NudP_ValueChanged);

            // lblP5Caption
            this.lblP5Caption.Text = "p5:";
            this.lblP5Caption.Location = new System.Drawing.Point(444, 15);
            this.lblP5Caption.AutoSize = true;

            // lblP5Value
            this.lblP5Value.Text = "0.20";
            this.lblP5Value.Location = new System.Drawing.Point(472, 15);
            this.lblP5Value.AutoSize = true;
            this.lblP5Value.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // lblN
            this.lblN.Text = "N:";
            this.lblN.Location = new System.Drawing.Point(540, 15);
            this.lblN.AutoSize = true;

            // nudN
            this.nudN.Location = new System.Drawing.Point(560, 12);
            this.nudN.Size = new System.Drawing.Size(90, 23);
            this.nudN.Maximum = 1000000;
            this.nudN.Minimum = 1;
            this.nudN.Value = 1000;

            // btnRun
            this.btnRun.Text = "Запуск";
            this.btnRun.Location = new System.Drawing.Point(660, 10);
            this.btnRun.Size = new System.Drawing.Size(75, 28);
            this.btnRun.Click += new EventHandler(this.BtnRun_Click);

            // btnRunAll
            this.btnRunAll.Text = "Запуск всех";
            this.btnRunAll.Location = new System.Drawing.Point(745, 10);
            this.btnRunAll.Size = new System.Drawing.Size(75, 28);
            this.btnRunAll.Click += new EventHandler(this.BtnRunAll_Click);

            // chart1
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            series1.Name = "Frequency";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            series1.IsValueShownAsLabel = true;
            series1.LabelFormat = "0.####";
            this.chart1.Series.Add(series1);
            this.chart1.Location = new System.Drawing.Point(12, 45);
            this.chart1.Size = new System.Drawing.Size(500, 300);

            // txtResults
            this.txtResults.Location = new System.Drawing.Point(520, 45);
            this.txtResults.Size = new System.Drawing.Size(310, 510);
            this.txtResults.Multiline = true;
            this.txtResults.ReadOnly = true;
            this.txtResults.ScrollBars = ScrollBars.Vertical;
            this.txtResults.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtResults.WordWrap = false;

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 570);
            this.Controls.Add(this.lblP1);
            this.Controls.Add(this.nudP1);
            this.Controls.Add(this.lblP2);
            this.Controls.Add(this.nudP2);
            this.Controls.Add(this.lblP3);
            this.Controls.Add(this.nudP3);
            this.Controls.Add(this.lblP4);
            this.Controls.Add(this.nudP4);
            this.Controls.Add(this.lblP5Caption);
            this.Controls.Add(this.lblP5Value);
            this.Controls.Add(this.lblN);
            this.Controls.Add(this.nudN);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.btnRunAll);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.txtResults);
            this.Text = "Лаб. 06 - Дискретные случайные величины";
            this.Name = "Form1";

            ((System.ComponentModel.ISupportInitialize)this.nudP1).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudP2).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudP3).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudP4).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.nudN).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.chart1).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Label lblP1;
        private Label lblP2;
        private Label lblP3;
        private Label lblP4;
        private Label lblP5Caption;
        private Label lblP5Value;
        private Label lblN;
        private NumericUpDown nudP1;
        private NumericUpDown nudP2;
        private NumericUpDown nudP3;
        private NumericUpDown nudP4;
        private NumericUpDown nudN;
        private Button btnRun;
        private Button btnRunAll;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private TextBox txtResults;
    }
}
