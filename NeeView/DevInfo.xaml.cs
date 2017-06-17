using NeeView.ComponentModel;
using NeeView.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace NeeView
{
    /// <summary>
    /// DevInfo.xaml の相互作用ロジック
    /// </summary>
    public partial class DevInfo : UserControl
    {
        public DevInfo()
        {
            InitializeComponent();
            this.Root.DataContext = new DevInfoViewModel();
        }
    }

    public class DevInfoViewModel : BindableBase
    {
        public Development Development => Development.Current;
        public JobEngine JobEngine => JobEngine.Current;
        public DragTransform DragTransform => DragTransform.Current;

        // 開発用：コンテンツ座標
        private Point _contentPosition;
        public Point ContentPosition
        {
            get { return _contentPosition; }
            set { _contentPosition = value; RaisePropertyChanged(); }
        }

        // 開発用：コンテンツ座標情報更新
        public void UpdateContentPosition()
        {
            ContentPosition = ContentCanvas.Current.MainContent.View.PointToScreen(new Point(0, 0));
        }


        // 開発用：
        ////public Development Development { get; private set; } = new Development();

        /// <summary>
        /// DevUpdateContentPosition command.
        /// </summary>
        private RelayCommand _DevUpdateContentPosition;
        public RelayCommand DevUpdateContentPosition
        {
            get { return _DevUpdateContentPosition = _DevUpdateContentPosition ?? new RelayCommand(DevUpdateContentPosition_Executed); }
        }

        private void DevUpdateContentPosition_Executed()
        {
            UpdateContentPosition();
        }
    }


    public class PointToDispStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point point)
            {
                return $"{(int)point.X,4},{(int)point.Y,4}";
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
