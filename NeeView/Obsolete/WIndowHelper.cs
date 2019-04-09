using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public static class WIndowHelper
    {
        public static void SetMainWindowOwner(this Window window)
        {
            window.Owner = App.Current?.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
    }
}
