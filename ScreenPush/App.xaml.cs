using HooksLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenPush
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        MainWindow mwnd;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mwnd = (MainWindow)App.Current.MainWindow;
        }


        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }

    }
}
