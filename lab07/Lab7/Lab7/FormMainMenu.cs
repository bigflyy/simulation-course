using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimulationLabs
{
    /// <summary>
    /// Главное меню лабораторной работы №7
    /// </summary>
    public class FormMainMenu : Form
    {
        public FormMainMenu()
        {
            // Настройки формы
            this.Text = "Лабораторная работа №7 — Марковская модель погоды";
            this.Size = new Size(450, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "Лабораторная работа №7",
                Location = new Point(100, 20),
                AutoSize = true,
                Font = new Font("Arial", 14, FontStyle.Bold)
            };
            this.Controls.Add(lblTitle);

            Label lblSubtitle = new Label
            {
                Text = "Марковская модель погоды",
                Location = new Point(110, 50),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Regular)
            };
            this.Controls.Add(lblSubtitle);

            // Кнопка запуска симуляции
            Button btnLab7 = new Button
            {
                Text = "Запустить симуляцию",
                Location = new Point(120, 90),
                Size = new Size(200, 45),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 11, FontStyle.Regular)
            };
            btnLab7.Click += (s, e) => {
                this.Hide();
                new FormLab7().ShowDialog();
                this.Show();
            };
            this.Controls.Add(btnLab7);

            // Кнопка закрытия
            Button btnClose = new Button
            {
                Text = "Выход",
                Location = new Point(170, 150),
                Size = new Size(100, 30)
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }
    }
}
