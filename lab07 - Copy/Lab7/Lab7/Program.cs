using System;
using System.Windows.Forms;

namespace SimulationLabs
{
    /// <summary>
    /// Точка входа приложения — запускает форму симуляции
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormLab7());
        }
    }
}
