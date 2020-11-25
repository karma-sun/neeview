using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
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
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// DebugInfo.xaml の相互作用ロジック
    /// </summary>
    public partial class DebugInfo : UserControl
    {
        public static DebugInfo Current { get; private set; }

        private DevInfoViewModel _vm;

        public DebugInfo()
        {
            Current = this;

            InitializeComponent();
            this.Root.DataContext = _vm = new DevInfoViewModel();

            _vm.WorkersChanged += (s, e) =>
            {
                this.items.Items.Refresh();
            };

            this.Loaded += (s, e) => _vm.OnLoad();
            this.Unloaded += (s, e) => _vm.OnUnload();
        }

        //
        [Conditional("DEBUG")]
        public void SetMessage(string message)
        {
            _vm.Message = message;
        }
    }

    public class DevInfoViewModel : BindableBase
    {
        public DevInfoViewModel()
        {
            JobEngine.Current.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(JobEngine.Workers))
                {
                    WorkersChanged?.Invoke(this, null);
                }
            };
        }

        public void OnLoad()
        {
            MainWindow.Current.MouseMove += MainWindow_MouseMove;
        }

        public void OnUnload()
        {
            MainWindow.Current.MouseMove -= MainWindow_MouseMove;
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            CursorPointWindow = e.GetPosition(MainWindow.Current);
            CursorPointRoot = e.GetPosition(MainWindow.Current.Root);
        }


        public event EventHandler WorkersChanged;


        public JobEngine JobEngine => JobEngine.Current;
        public DragTransform DragTransform => ViewComponentProvider.Current.GetViewComponent().DragTransform;
        public BookOperation BookOperation => BookOperation.Current;
        public BookHub BookHub => BookHub.Current;
        public ContentRebuild ContentRebuild => ViewComponentProvider.Current.GetViewComponent().ContentRebuild;


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
            ContentPosition = ViewComponentProvider.Current.GetViewComponent().ContentCanvas.MainContent.View.PointToScreen(new Point(0, 0));
        }

        /// <summary>
        /// Message property.
        /// </summary>
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { if (_Message != value) { _Message = value; RaisePropertyChanged(); } }
        }


        private Point _CursorPointWindow;
        public Point CursorPointWindow
        {
            get { return _CursorPointWindow; }
            set { SetProperty(ref _CursorPointWindow, value); }
        }


        private Point _CursorPointRoot;
        public Point CursorPointRoot
        {
            get { return _CursorPointRoot; }
            set { SetProperty(ref _CursorPointRoot, value); }
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
