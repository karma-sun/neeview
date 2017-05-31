using NeeView.CommandLine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView.Lab
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //
            var option = new SampleOptionTest();
            option.Exec(e.Args);


            // メインウィンドウ起動
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
