using System;
using System.Windows.Forms;

namespace SimulationLabs
{
    /// <summary>
    /// Точка входа приложения — запускает главное меню
    /// </summary>
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMainMenu());
        }
    }
}
