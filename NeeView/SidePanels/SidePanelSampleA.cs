using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView
{
    public class SidePanelSampleA : IPanel, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string TypeCode => "SampleA";

        public ImageSource Icon { get; private set; }
        public Thickness IconMargin { get; private set; }

        public string IconTips => "ファイル情報";


        public FrameworkElement View { get; private set; }


        public SidePanelSampleA()
        {
            Icon = new BitmapImage(new Uri(@"E:\Pictures\倉庫\i.png"));
            //Icon = App.Current.MainWindow.Resources["ic_info_outline_24px"] as DrawingImage;
            IconMargin = new Thickness(8);

            var rectangle = new Rectangle();
            rectangle.Fill = Brushes.Red;
            rectangle.Width = 100;
            rectangle.Height = 100;
            View = rectangle;
        }

        //
        public void Reflesh()
        {
            Debug.WriteLine("REFLESH!!"); 
        }
    }
}
