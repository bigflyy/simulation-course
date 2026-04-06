using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimulationLabs
{
    public class FormMainMenu : Form
    {
        public FormMainMenu()
        {
            this.Text = "Лабораторные работы 06";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label
            {
                Text = "Выберите лабораторную работу",
                Location = new Point(80, 30),
                AutoSize = true,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            this.Controls.Add(lblTitle);

            Button btnLab6_1 = new Button
            {
                Text = "Lab 06-1: Дискретная СВ",
                Location = new Point(100, 80),
                Size = new Size(180, 40),
                BackColor = Color.LightBlue
            };
            btnLab6_1.Click += (s, e) => {
                this.Hide();
                new FormLab6_1().ShowDialog();
                this.Show();
            };
            this.Controls.Add(btnLab6_1);

            Button btnLab6_2 = new Button
            {
                Text = "Lab 06-2: Нормальная СВ",
                Location = new Point(100, 140),
                Size = new Size(180, 40),
                BackColor = Color.LightGreen
            };
            btnLab6_2.Click += (s, e) => {
                this.Hide();
                new FormLab6_2().ShowDialog();
                this.Show();
            };
            this.Controls.Add(btnLab6_2);
        }
    }
}