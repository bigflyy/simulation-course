namespace RandomEvents
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            tabControl = new TabControl();
            tabYesNo = new TabPage();
            lblQuestion = new Label();
            txtQuestion = new TextBox();
            lblProbability = new Label();
            nudProbability = new NumericUpDown();
            btnAnswer = new Button();
            lblResult = new Label();
            tabMagicBall = new TabPage();
            label1 = new Label();
            nudP5 = new NumericUpDown();
            label2 = new Label();
            nudP6 = new NumericUpDown();
            label3 = new Label();
            nudP7 = new NumericUpDown();
            lblP1 = new Label();
            nudP1 = new NumericUpDown();
            lblP2 = new Label();
            nudP2 = new NumericUpDown();
            lblP3 = new Label();
            nudP3 = new NumericUpDown();
            lblP4 = new Label();
            nudP4 = new NumericUpDown();
            lblP5 = new Label();
            lblP8Value = new Label();
            btnRun = new Button();
            txtResults = new TextBox();
            tabControl.SuspendLayout();
            tabYesNo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudProbability).BeginInit();
            tabMagicBall.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudP5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudP6).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudP7).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudP1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudP2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudP3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudP4).BeginInit();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabYesNo);
            tabControl.Controls.Add(tabMagicBall);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Margin = new Padding(4);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(780, 676);
            tabControl.TabIndex = 0;
            // 
            // tabYesNo
            // 
            tabYesNo.Controls.Add(lblQuestion);
            tabYesNo.Controls.Add(txtQuestion);
            tabYesNo.Controls.Add(lblProbability);
            tabYesNo.Controls.Add(nudProbability);
            tabYesNo.Controls.Add(btnAnswer);
            tabYesNo.Controls.Add(lblResult);
            tabYesNo.Location = new Point(4, 34);
            tabYesNo.Margin = new Padding(4);
            tabYesNo.Name = "tabYesNo";
            tabYesNo.Padding = new Padding(15);
            tabYesNo.Size = new Size(772, 638);
            tabYesNo.TabIndex = 0;
            tabYesNo.Text = "Задание 5.1 - Да или Нет";
            // 
            // lblQuestion
            // 
            lblQuestion.AutoSize = true;
            lblQuestion.Location = new Point(25, 25);
            lblQuestion.Margin = new Padding(4, 0, 4, 0);
            lblQuestion.Name = "lblQuestion";
            lblQuestion.Size = new Size(115, 25);
            lblQuestion.TabIndex = 0;
            lblQuestion.Text = "Ваш вопрос:";
            // 
            // txtQuestion
            // 
            txtQuestion.Location = new Point(25, 56);
            txtQuestion.Margin = new Padding(4);
            txtQuestion.Name = "txtQuestion";
            txtQuestion.Size = new Size(499, 31);
            txtQuestion.TabIndex = 1;
            txtQuestion.Text = "Жив ли кот Шрёдингера?";
            // 
            // lblProbability
            // 
            lblProbability.AutoSize = true;
            lblProbability.Location = new Point(25, 106);
            lblProbability.Margin = new Padding(4, 0, 4, 0);
            lblProbability.Name = "lblProbability";
            lblProbability.Size = new Size(173, 25);
            lblProbability.TabIndex = 2;
            lblProbability.Text = "Вероятность ДА (p):";
            // 
            // nudProbability
            // 
            nudProbability.DecimalPlaces = 2;
            nudProbability.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudProbability.Location = new Point(25, 138);
            nudProbability.Margin = new Padding(4);
            nudProbability.Maximum = new decimal(new int[] { 100, 0, 0, 131072 });
            nudProbability.Name = "nudProbability";
            nudProbability.Size = new Size(125, 31);
            nudProbability.TabIndex = 3;
            nudProbability.Value = new decimal(new int[] { 50, 0, 0, 131072 });
            // 
            // btnAnswer
            // 
            btnAnswer.Location = new Point(25, 194);
            btnAnswer.Margin = new Padding(4);
            btnAnswer.Name = "btnAnswer";
            btnAnswer.Size = new Size(150, 50);
            btnAnswer.TabIndex = 4;
            btnAnswer.Text = "Ответ!";
            btnAnswer.UseVisualStyleBackColor = true;
            // 
            // lblResult
            // 
            lblResult.Font = new Font("Segoe UI", 36F, FontStyle.Bold, GraphicsUnit.Point);
            lblResult.Location = new Point(25, 262);
            lblResult.Margin = new Padding(4, 0, 4, 0);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(700, 125);
            lblResult.TabIndex = 5;
            lblResult.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // tabMagicBall
            // 
            tabMagicBall.Controls.Add(label1);
            tabMagicBall.Controls.Add(nudP5);
            tabMagicBall.Controls.Add(label2);
            tabMagicBall.Controls.Add(nudP6);
            tabMagicBall.Controls.Add(label3);
            tabMagicBall.Controls.Add(nudP7);
            tabMagicBall.Controls.Add(lblP1);
            tabMagicBall.Controls.Add(nudP1);
            tabMagicBall.Controls.Add(lblP2);
            tabMagicBall.Controls.Add(nudP2);
            tabMagicBall.Controls.Add(lblP3);
            tabMagicBall.Controls.Add(nudP3);
            tabMagicBall.Controls.Add(lblP4);
            tabMagicBall.Controls.Add(nudP4);
            tabMagicBall.Controls.Add(lblP5);
            tabMagicBall.Controls.Add(lblP8Value);
            tabMagicBall.Controls.Add(btnRun);
            tabMagicBall.Controls.Add(txtResults);
            tabMagicBall.Location = new Point(4, 34);
            tabMagicBall.Margin = new Padding(4);
            tabMagicBall.Name = "tabMagicBall";
            tabMagicBall.Padding = new Padding(15);
            tabMagicBall.Size = new Size(772, 638);
            tabMagicBall.TabIndex = 1;
            tabMagicBall.Text = "Задание 5.2 - Шар предсказаний";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 178);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(195, 25);
            label1.TabIndex = 12;
            label1.Text = "p5 (Определённо нет):";
            // 
            // nudP5
            // 
            nudP5.DecimalPlaces = 3;
            nudP5.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudP5.Location = new Point(325, 178);
            nudP5.Margin = new Padding(4);
            nudP5.Maximum = new decimal(new int[] { 100, 0, 0, 131072 });
            nudP5.Name = "nudP5";
            nudP5.Size = new Size(100, 31);
            nudP5.TabIndex = 13;
            nudP5.Value = new decimal(new int[] { 125, 0, 0, 196608 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(25, 218);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(169, 25);
            label2.TabIndex = 14;
            label2.Text = "p6 (Спроси позже):";
            // 
            // nudP6
            // 
            nudP6.DecimalPlaces = 3;
            nudP6.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudP6.Location = new Point(325, 218);
            nudP6.Margin = new Padding(4);
            nudP6.Maximum = new decimal(new int[] { 100, 0, 0, 131072 });
            nudP6.Name = "nudP6";
            nudP6.Size = new Size(100, 31);
            nudP6.TabIndex = 15;
            nudP6.Value = new decimal(new int[] { 125, 0, 0, 196608 });
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(25, 258);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(199, 25);
            label3.TabIndex = 16;
            label3.Text = "p7 (И не рассчитывай):";
            // 
            // nudP7
            // 
            nudP7.DecimalPlaces = 3;
            nudP7.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudP7.Location = new Point(325, 256);
            nudP7.Margin = new Padding(4);
            nudP7.Maximum = new decimal(new int[] { 100, 0, 0, 131072 });
            nudP7.Name = "nudP7";
            nudP7.Size = new Size(100, 31);
            nudP7.TabIndex = 17;
            nudP7.Value = new decimal(new int[] { 125, 0, 0, 196608 });
            // 
            // lblP1
            // 
            lblP1.AutoSize = true;
            lblP1.Location = new Point(25, 22);
            lblP1.Margin = new Padding(4, 0, 4, 0);
            lblP1.Name = "lblP1";
            lblP1.Size = new Size(188, 25);
            lblP1.TabIndex = 0;
            lblP1.Text = "p1 (Определённо да):";
            // 
            // nudP1
            // 
            nudP1.DecimalPlaces = 3;
            nudP1.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudP1.Location = new Point(325, 19);
            nudP1.Margin = new Padding(4);
            nudP1.Maximum = new decimal(new int[] { 100, 0, 0, 131072 });
            nudP1.Name = "nudP1";
            nudP1.Size = new Size(100, 31);
            nudP1.TabIndex = 1;
            nudP1.Value = new decimal(new int[] { 125, 0, 0, 196608 });
            // 
            // lblP2
            // 
            lblP2.AutoSize = true;
            lblP2.Location = new Point(25, 62);
            lblP2.Margin = new Padding(4, 0, 4, 0);
            lblP2.Name = "lblP2";
            lblP2.Size = new Size(154, 25);
            lblP2.TabIndex = 2;
            lblP2.Text = "p2 (Вероятно да):";
            // 
            // nudP2
            // 
            nudP2.DecimalPlaces = 3;
            nudP2.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudP2.Location = new Point(325, 59);
            nudP2.Margin = new Padding(4);
            nudP2.Maximum = new decimal(new int[] { 100, 0, 0, 131072 });
            nudP2.Name = "nudP2";
            nudP2.Size = new Size(100, 31);
            nudP2.TabIndex = 3;
            nudP2.Value = new decimal(new int[] { 125, 0, 0, 196608 });
            // 
            // lblP3
            // 
            lblP3.AutoSize = true;
            lblP3.Location = new Point(25, 102);
            lblP3.Margin = new Padding(4, 0, 4, 0);
            lblP3.Name = "lblP3";
            lblP3.Size = new Size(139, 25);
            lblP3.TabIndex = 4;
            lblP3.Text = "p3 (Возможно):";
            // 
            // nudP3
            // 
            nudP3.DecimalPlaces = 3;
            nudP3.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudP3.Location = new Point(325, 99);
            nudP3.Margin = new Padding(4);
            nudP3.Maximum = new decimal(new int[] { 100, 0, 0, 131072 });
            nudP3.Name = "nudP3";
            nudP3.Size = new Size(100, 31);
            nudP3.TabIndex = 5;
            nudP3.Value = new decimal(new int[] { 125, 0, 0, 196608 });
            // 
            // lblP4
            // 
            lblP4.AutoSize = true;
            lblP4.Location = new Point(25, 139);
            lblP4.Margin = new Padding(4, 0, 4, 0);
            lblP4.Name = "lblP4";
            lblP4.Size = new Size(161, 25);
            lblP4.TabIndex = 6;
            lblP4.Text = "p4 (Вероятно нет):";
            // 
            // nudP4
            // 
            nudP4.DecimalPlaces = 3;
            nudP4.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudP4.Location = new Point(325, 139);
            nudP4.Margin = new Padding(4);
            nudP4.Maximum = new decimal(new int[] { 100, 0, 0, 131072 });
            nudP4.Name = "nudP4";
            nudP4.Size = new Size(100, 31);
            nudP4.TabIndex = 7;
            nudP4.Value = new decimal(new int[] { 125, 0, 0, 196608 });
            // 
            // lblP5
            // 
            lblP5.AutoSize = true;
            lblP5.Location = new Point(25, 294);
            lblP5.Margin = new Padding(4, 0, 4, 0);
            lblP5.Name = "lblP5";
            lblP5.Size = new Size(216, 25);
            lblP5.TabIndex = 8;
            lblP5.Text = "p8 (Очень сомнительно):";
            // 
            // lblP8Value
            // 
            lblP8Value.AutoSize = true;
            lblP8Value.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            lblP8Value.Location = new Point(325, 294);
            lblP8Value.Margin = new Padding(4, 0, 4, 0);
            lblP8Value.Name = "lblP8Value";
            lblP8Value.Size = new Size(57, 25);
            lblP8Value.TabIndex = 9;
            lblP8Value.Text = "0.125";
            // 
            // btnRun
            // 
            btnRun.Location = new Point(25, 362);
            btnRun.Margin = new Padding(4);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(200, 50);
            btnRun.TabIndex = 10;
            btnRun.Text = "Предсказание";
            btnRun.UseVisualStyleBackColor = true;
            // 
            // txtResults
            // 
            txtResults.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point);
            txtResults.Location = new Point(19, 477);
            txtResults.Margin = new Padding(4);
            txtResults.Multiline = true;
            txtResults.Name = "txtResults";
            txtResults.ReadOnly = true;
            txtResults.Size = new Size(699, 100);
            txtResults.TabIndex = 11;
            txtResults.TextAlign = HorizontalAlignment.Center;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(780, 676);
            Controls.Add(tabControl);
            Margin = new Padding(4);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Лаб. 05 - Случайные события";
            tabControl.ResumeLayout(false);
            tabYesNo.ResumeLayout(false);
            tabYesNo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudProbability).EndInit();
            tabMagicBall.ResumeLayout(false);
            tabMagicBall.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudP5).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudP6).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudP7).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudP1).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudP2).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudP3).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudP4).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl;
        private TabPage tabYesNo;
        private TabPage tabMagicBall;

        // Tab 1
        private Label lblQuestion;
        private TextBox txtQuestion;
        private Label lblProbability;
        private NumericUpDown nudProbability;
        private Button btnAnswer;
        private Label lblResult;

        // Tab 2
        private Label lblP1;
        private Label lblP2;
        private Label lblP3;
        private Label lblP4;
        private Label lblP5;
        private NumericUpDown nudP1;
        private NumericUpDown nudP2;
        private NumericUpDown nudP3;
        private NumericUpDown nudP4;
        private Label lblP8Value;
        private Button btnRun;
        private TextBox txtResults;
        private Label label1;
        private NumericUpDown nudP5;
        private Label label2;
        private NumericUpDown nudP6;
        private Label label3;
        private NumericUpDown nudP7;
    }
}
