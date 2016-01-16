using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AppWpf.Views;
using AppWpf.ViewModels;

namespace AppWpf
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //Hauptfenster
            var mainWindow = new MainWindow();
            this.MainWindow = mainWindow;

            //HauptViewmodel zuweisen
            var mainWindowViewModel = new MainWindowViewModel();
            mainWindow.DataContext = mainWindowViewModel;

            mainWindow.Show();
        }
    }
}
