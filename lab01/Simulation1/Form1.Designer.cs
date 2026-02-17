namespace Simulation1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            labelHeight = new Label();
            numericHeight = new NumericUpDown();
            numericAngle = new NumericUpDown();
            labelAngle = new Label();
            numericSpeed = new NumericUpDown();
            labelSpeed = new Label();
            numericWeight = new NumericUpDown();
            label3 = new Label();
            numericSize = new NumericUpDown();
            labelSize = new Label();
            buttonLaunch = new Button();
            chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            panelControls = new Panel();
            buttonClear = new Button();
            labelStep = new Label();
            numericStep = new NumericUpDown();
            textBoxResults = new TextBox();
            ((System.ComponentModel.ISupportInitialize)numericHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericAngle).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericSpeed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericWeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chart1).BeginInit();
            panelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericStep).BeginInit();
            SuspendLayout();
            // 
            // labelHeight
            // 
            labelHeight.AutoSize = true;
            labelHeight.Location = new Point(11, 13);
            labelHeight.Name = "labelHeight";
            labelHeight.Size = new Size(65, 25);
            labelHeight.TabIndex = 0;
            labelHeight.Text = "Height";
            // 
            // numericHeight
            // 
            numericHeight.Location = new Point(82, 11);
            numericHeight.Name = "numericHeight";
            numericHeight.Size = new Size(180, 31);
            numericHeight.TabIndex = 1;
            // 
            // numericAngle
            // 
            numericAngle.Location = new Point(82, 44);
            numericAngle.Name = "numericAngle";
            numericAngle.Size = new Size(180, 31);
            numericAngle.TabIndex = 3;
            // 
            // labelAngle
            // 
            labelAngle.AutoSize = true;
            labelAngle.Location = new Point(11, 46);
            labelAngle.Name = "labelAngle";
            labelAngle.Size = new Size(58, 25);
            labelAngle.TabIndex = 2;
            labelAngle.Text = "Angle";
            // 
            // numericSpeed
            // 
            numericSpeed.Location = new Point(82, 78);
            numericSpeed.Name = "numericSpeed";
            numericSpeed.Size = new Size(180, 31);
            numericSpeed.TabIndex = 5;
            // 
            // labelSpeed
            // 
            labelSpeed.AutoSize = true;
            labelSpeed.Location = new Point(11, 78);
            labelSpeed.Name = "labelSpeed";
            labelSpeed.Size = new Size(62, 25);
            labelSpeed.TabIndex = 4;
            labelSpeed.Text = "Speed";
            // 
            // numericWeight
            // 
            numericWeight.Location = new Point(360, 48);
            numericWeight.Name = "numericWeight";
            numericWeight.Size = new Size(180, 31);
            numericWeight.TabIndex = 9;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(289, 54);
            label3.Name = "label3";
            label3.Size = new Size(68, 25);
            label3.TabIndex = 8;
            label3.Text = "Weight";
            // 
            // numericSize
            // 
            numericSize.DecimalPlaces = 2;
            numericSize.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericSize.Location = new Point(360, 11);
            numericSize.Name = "numericSize";
            numericSize.Size = new Size(180, 31);
            numericSize.TabIndex = 7;
            // 
            // labelSize
            // 
            labelSize.AutoSize = true;
            labelSize.Location = new Point(289, 17);
            labelSize.Name = "labelSize";
            labelSize.Size = new Size(43, 25);
            labelSize.TabIndex = 6;
            labelSize.Text = "Size";
            // 
            // buttonLaunch
            // 
            buttonLaunch.Location = new Point(575, 13);
            buttonLaunch.Name = "buttonLaunch";
            buttonLaunch.Size = new Size(112, 34);
            buttonLaunch.TabIndex = 10;
            buttonLaunch.Text = "Launch";
            buttonLaunch.UseVisualStyleBackColor = true;
            buttonLaunch.Click += buttonLaunch_Click;
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea1);
            chart1.Dock = DockStyle.Top;
            legend1.Name = "Legend1";
            chart1.Legends.Add(legend1);
            chart1.Location = new Point(0, 0);
            chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            chart1.Series.Add(series1);
            chart1.Size = new Size(800, 419);
            chart1.TabIndex = 11;
            chart1.Text = "chart1";
            // 
            // panelControls
            // 
            panelControls.Controls.Add(labelHeight);
            panelControls.Controls.Add(numericHeight);
            panelControls.Controls.Add(buttonLaunch);
            panelControls.Controls.Add(buttonClear);
            panelControls.Controls.Add(labelAngle);
            panelControls.Controls.Add(numericSize);
            panelControls.Controls.Add(numericWeight);
            panelControls.Controls.Add(numericAngle);
            panelControls.Controls.Add(label3);
            panelControls.Controls.Add(labelSpeed);
            panelControls.Controls.Add(numericSpeed);
            panelControls.Controls.Add(labelSize);
            panelControls.Controls.Add(labelStep);
            panelControls.Controls.Add(numericStep);
            panelControls.Dock = DockStyle.Top;
            panelControls.Location = new Point(0, 419);
            panelControls.Name = "panelControls";
            panelControls.Size = new Size(800, 128);
            panelControls.TabIndex = 12;
            // 
            // buttonClear
            // 
            buttonClear.Location = new Point(575, 55);
            buttonClear.Name = "buttonClear";
            buttonClear.Size = new Size(112, 34);
            buttonClear.TabIndex = 15;
            buttonClear.Text = "Clear";
            buttonClear.UseVisualStyleBackColor = true;
            buttonClear.Click += buttonClear_Click;
            // 
            // labelStep
            // 
            labelStep.AutoSize = true;
            labelStep.Location = new Point(289, 88);
            labelStep.Name = "labelStep";
            labelStep.Size = new Size(47, 25);
            labelStep.TabIndex = 13;
            labelStep.Text = "Step";
            // 
            // numericStep
            // 
            numericStep.DecimalPlaces = 4;
            numericStep.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericStep.Location = new Point(360, 85);
            numericStep.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numericStep.Minimum = new decimal(new int[] { 1, 0, 0, 262144 });
            numericStep.Name = "numericStep";
            numericStep.Size = new Size(180, 31);
            numericStep.TabIndex = 14;
            numericStep.Value = new decimal(new int[] { 1, 0, 0, 131072 });
            // 
            // textBoxResults
            // 
            textBoxResults.Dock = DockStyle.Fill;
            textBoxResults.Font = new Font("Consolas", 9F);
            textBoxResults.Location = new Point(0, 547);
            textBoxResults.Multiline = true;
            textBoxResults.Name = "textBoxResults";
            textBoxResults.ReadOnly = true;
            textBoxResults.ScrollBars = ScrollBars.Vertical;
            textBoxResults.Size = new Size(800, 153);
            textBoxResults.TabIndex = 16;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 700);
            Controls.Add(textBoxResults);
            Controls.Add(panelControls);
            Controls.Add(chart1);
            Name = "Form1";
            Text = "Simulation";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)numericHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericAngle).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericSpeed).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericWeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)chart1).EndInit();
            panelControls.ResumeLayout(false);
            panelControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericStep).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelHeight;
        private NumericUpDown numericHeight;
        private NumericUpDown numericAngle;
        private Label labelAngle;
        private NumericUpDown numericSpeed;
        private Label labelSpeed;
        private NumericUpDown numericWeight;
        private Label label3;
        private NumericUpDown numericSize;
        private Label labelSize;
        private Button buttonLaunch;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private Panel panelControls;
        private Label labelStep;
        private NumericUpDown numericStep;
        private Button buttonClear;
        private TextBox textBoxResults;
    }
}
