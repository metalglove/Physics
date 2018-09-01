using System.Windows;
using Physics.Helpers;
using Physics.Services;
using Physics.ViewModels;

namespace Physics
{
    public partial class App : Application
    {
        internal static readonly ServiceProvider serviceProvider = new ServiceProvider();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();

            NavigationService navigationService = serviceProvider.GetService<NavigationService>();
            mainWindow.DataContext = navigationService;

            navigationService.NavigateTo<ProjectileLauncherViewModel>();
            mainWindow.Show();
        }
    }
}
