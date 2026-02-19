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
            chartTrajectory = new System.Windows.Forms.DataVisualization.Charting.Chart();
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
            ((System.ComponentModel.ISupportInitialize)chartTrajectory).BeginInit();
            panelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericStep).BeginInit();
            SuspendLayout();
            // 
            // labelHeight
            // 
            labelHeight.AutoSize = true;
            labelHeight.Location = new Point(8, 8);
            labelHeight.Margin = new Padding(2, 0, 2, 0);
            labelHeight.Name = "labelHeight";
            labelHeight.Size = new Size(43, 15);
            labelHeight.TabIndex = 0;
            labelHeight.Text = "Height";
            // 
            // numericHeight
            // 
            numericHeight.Location = new Point(57, 7);
            numericHeight.Margin = new Padding(2, 2, 2, 2);
            numericHeight.Name = "numericHeight";
            numericHeight.Size = new Size(126, 23);
            numericHeight.TabIndex = 1;
            // 
            // numericAngle
            // 
            numericAngle.Location = new Point(57, 26);
            numericAngle.Margin = new Padding(2, 2, 2, 2);
            numericAngle.Name = "numericAngle";
            numericAngle.Size = new Size(126, 23);
            numericAngle.TabIndex = 3;
            // 
            // labelAngle
            // 
            labelAngle.AutoSize = true;
            labelAngle.Location = new Point(8, 28);
            labelAngle.Margin = new Padding(2, 0, 2, 0);
            labelAngle.Name = "labelAngle";
            labelAngle.Size = new Size(38, 15);
            labelAngle.TabIndex = 2;
            labelAngle.Text = "Angle";
            // 
            // numericSpeed
            // 
            numericSpeed.Location = new Point(57, 47);
            numericSpeed.Margin = new Padding(2, 2, 2, 2);
            numericSpeed.Name = "numericSpeed";
            numericSpeed.Size = new Size(126, 23);
            numericSpeed.TabIndex = 5;
            // 
            // labelSpeed
            // 
            labelSpeed.AutoSize = true;
            labelSpeed.Location = new Point(8, 47);
            labelSpeed.Margin = new Padding(2, 0, 2, 0);
            labelSpeed.Name = "labelSpeed";
            labelSpeed.Size = new Size(39, 15);
            labelSpeed.TabIndex = 4;
            labelSpeed.Text = "Speed";
            // 
            // numericWeight
            // 
            numericWeight.Location = new Point(252, 29);
            numericWeight.Margin = new Padding(2, 2, 2, 2);
            numericWeight.Name = "numericWeight";
            numericWeight.Size = new Size(126, 23);
            numericWeight.TabIndex = 9;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(202, 32);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(45, 15);
            label3.TabIndex = 8;
            label3.Text = "Weight";
            // 
            // numericSize
            // 
            numericSize.DecimalPlaces = 2;
            numericSize.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericSize.Location = new Point(252, 7);
            numericSize.Margin = new Padding(2, 2, 2, 2);
            numericSize.Name = "numericSize";
            numericSize.Size = new Size(126, 23);
            numericSize.TabIndex = 7;
            // 
            // labelSize
            // 
            labelSize.AutoSize = true;
            labelSize.Location = new Point(202, 10);
            labelSize.Margin = new Padding(2, 0, 2, 0);
            labelSize.Name = "labelSize";
            labelSize.Size = new Size(27, 15);
            labelSize.TabIndex = 6;
            labelSize.Text = "Size";
            // 
            // buttonLaunch
            // 
            buttonLaunch.Location = new Point(402, 8);
            buttonLaunch.Margin = new Padding(2, 2, 2, 2);
            buttonLaunch.Name = "buttonLaunch";
            buttonLaunch.Size = new Size(78, 20);
            buttonLaunch.TabIndex = 10;
            buttonLaunch.Text = "Launch";
            buttonLaunch.UseVisualStyleBackColor = true;
            buttonLaunch.Click += buttonLaunch_Click;
            // 
            // chartTrajectory
            // 
            chartArea1.Name = "ChartArea1";
            chartTrajectory.ChartAreas.Add(chartArea1);
            chartTrajectory.Dock = DockStyle.Top;
            legend1.Name = "Legend1";
            chartTrajectory.Legends.Add(legend1);
            chartTrajectory.Location = new Point(0, 0);
            chartTrajectory.Margin = new Padding(2, 2, 2, 2);
            chartTrajectory.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Trajectory";
            chartTrajectory.Series.Add(series1);
            chartTrajectory.Size = new Size(560, 251);
            chartTrajectory.TabIndex = 11;
            chartTrajectory.Text = "chart1";
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
            panelControls.Location = new Point(0, 251);
            panelControls.Margin = new Padding(2, 2, 2, 2);
            panelControls.Name = "panelControls";
            panelControls.Size = new Size(560, 77);
            panelControls.TabIndex = 12;
            // 
            // buttonClear
            // 
            buttonClear.Location = new Point(402, 33);
            buttonClear.Margin = new Padding(2, 2, 2, 2);
            buttonClear.Name = "buttonClear";
            buttonClear.Size = new Size(78, 20);
            buttonClear.TabIndex = 15;
            buttonClear.Text = "Clear";
            buttonClear.UseVisualStyleBackColor = true;
            buttonClear.Click += buttonClear_Click;
            // 
            // labelStep
            // 
            labelStep.AutoSize = true;
            labelStep.Location = new Point(202, 53);
            labelStep.Margin = new Padding(2, 0, 2, 0);
            labelStep.Name = "labelStep";
            labelStep.Size = new Size(30, 15);
            labelStep.TabIndex = 13;
            labelStep.Text = "Step";
            // 
            // numericStep
            // 
            numericStep.DecimalPlaces = 4;
            numericStep.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            numericStep.Location = new Point(252, 51);
            numericStep.Margin = new Padding(2, 2, 2, 2);
            numericStep.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numericStep.Minimum = new decimal(new int[] { 1, 0, 0, 262144 });
            numericStep.Name = "numericStep";
            numericStep.Size = new Size(126, 23);
            numericStep.TabIndex = 14;
            numericStep.Value = new decimal(new int[] { 1, 0, 0, 131072 });
            // 
            // textBoxResults
            // 
            textBoxResults.Dock = DockStyle.Fill;
            textBoxResults.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);
            textBoxResults.Location = new Point(0, 328);
            textBoxResults.Margin = new Padding(2, 2, 2, 2);
            textBoxResults.Multiline = true;
            textBoxResults.Name = "textBoxResults";
            textBoxResults.ReadOnly = true;
            textBoxResults.ScrollBars = ScrollBars.Vertical;
            textBoxResults.Size = new Size(560, 92);
            textBoxResults.TabIndex = 16;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 420);
            Controls.Add(textBoxResults);
            Controls.Add(panelControls);
            Controls.Add(chartTrajectory);
            Margin = new Padding(2, 2, 2, 2);
            Name = "Form1";
            Text = "Simulation";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)numericHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericAngle).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericSpeed).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericWeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)chartTrajectory).EndInit();
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
        private System.Windows.Forms.DataVisualization.Charting.Chart chartTrajectory;
        private Panel panelControls;
        private Label labelStep;
        private NumericUpDown numericStep;
        private Button buttonClear;
        private TextBox textBoxResults;
    }
}
