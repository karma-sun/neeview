using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
#if false
        public static JobEngine JobEngine { get; private set; }

        public static Susie.Susie Susie { get; private set; }

        static App()
        {
        }

        public static void StartEx()
        { 
            JobEngine = new JobEngine();
            JobEngine.Start();

            Susie = new Susie.Susie();
        }
#endif
    }
}
