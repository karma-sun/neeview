using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using NeeView;

namespace NeeView.Lab
{
    public class PanelSampleA : IPanel, INotifyPropertyChanged
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


        public bool IsVisibleLock => false;



        public PanelSampleA()
        {
            //Icon = new BitmapImage(new Uri(@"E:\Pictures\倉庫\i.png"));
            Icon = App.Current.MainWindow.Resources["ic_info_outline_24px"] as DrawingImage;
            IconMargin = new Thickness(8);

            var rectangle = new Rectangle();
            rectangle.Fill = Brushes.Red;
            rectangle.Width = 100;
            rectangle.Height = 100;
            View = rectangle;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class PanelSampleB : IPanel, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string TypeCode => "SampleB";

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "履歴";

        public FrameworkElement View { get; private set; }

        public bool IsVisibleLock => false;

        public PanelSampleB()
        {
            //Icon = new BitmapImage(new Uri(@"E:\Pictures\倉庫\NeeView1.5.png"));
            Icon = App.Current.MainWindow.Resources["ic_history_24px"] as DrawingImage;

            IconMargin = new Thickness(6, 8, 10, 8);


            var rectangle = new Rectangle();
            rectangle.Fill = Brushes.Blue;
            rectangle.Width = 100;
            rectangle.Height = 100;
            View = rectangle;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class PanelSampleC : IPanel, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string TypeCode => "SampleC";

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "フォルダーリスト";


        public FrameworkElement View { get; private set; }

        public bool IsVisibleLock => false;


        public PanelSampleC()
        {
            //Icon = new BitmapImage(new Uri(@"E:\Pictures\倉庫\key.png"));

            Icon = App.Current.MainWindow.Resources["ic_folder_48px"] as DrawingImage;
            //Icon = App.Current.Resources["ic_folder_48px_"] as DrawingImage;

            IconMargin = new Thickness(10);

            var rectangle = new Rectangle();
            rectangle.Fill = Brushes.Yellow;
            rectangle.Width = 100;
            rectangle.Height = 100;
            View = rectangle;
        }
    }
}
