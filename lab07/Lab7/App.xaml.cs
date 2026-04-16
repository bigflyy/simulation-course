using System.Windows;

namespace SimulationLabs
{
    /// <summary>
    /// Точка входа приложения — лабораторная работа 7: Марковская модель погоды.
    /// Запускает главное окно WPF.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
