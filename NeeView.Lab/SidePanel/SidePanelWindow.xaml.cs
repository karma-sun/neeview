using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

using NeeView;

namespace NeeView.Lab
{
    /// <summary>
    /// SidePanelWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SidePanelWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// BgBrush property.
        /// </summary>
        public Brush BgBrush
        {
            get { return _BgBrush; }
            set { if (_BgBrush != value) { _BgBrush = value; RaisePropertyChanged(); } }
        }

        //
        private Brush _BgBrush = Brushes.LightBlue;



        //
        public SidePanelWindow()
        {
            InitializeComponent();
            InitializeSidePanel();

            this.DataContext = this;
        }

        //
        private List<IPanel> _panels;


        /// <summary>
        /// SidePanel property.
        /// </summary>
        public SidePanelFrameModel SidePanel
        {
            get { return _sidePanel; }
            set { if (_sidePanel != value) { _sidePanel = value; RaisePropertyChanged(); } }
        }

        //
        private SidePanelFrameModel _sidePanel;


        //
        private void InitializeSidePanel()
        {
            _panels = new List<IPanel>()
            {
                new PanelSampleA(),
                new PanelSampleB(),
                new PanelSampleC(),
            };

            _sidePanel = new SidePanelFrameModel();
            _sidePanel.Restore(null, _panels);

            this.SidePanelFrame.Model = _sidePanel;
        }


        /// <summary>
        /// TestString property.
        /// </summary>
        public string TestString
        {
            get { return _TestString; }
            set { if (_TestString != value) { _TestString = value; RaisePropertyChanged(); } }
        }

        //
        private string _TestString = "TEST!!";


        /// <summary>
        /// SidePanelMargin property.
        /// </summary>
        public Thickness SidePanelMargin
        {
            get { return _SidePanelMargin; }
            set { if (_SidePanelMargin != value) { _SidePanelMargin = value; RaisePropertyChanged(); } }
        }

        //
        private Thickness _SidePanelMargin;

        //
        private void MarginMenu_Checked(object sender, RoutedEventArgs e)
        {
            SidePanelMargin = new Thickness(0, 20, 0, 0);
        }

        //
        private void MarginMenu_Unchecked(object sender, RoutedEventArgs e)
        {
            SidePanelMargin = default(Thickness);
        }


        private SidePanelFrameModel.Memento _memento;

        private void MementoMenu_Click(object sender, RoutedEventArgs e)
        {
            _memento = this.SidePanel.CreateMemento();

            var memento = new SidePanelFrameModel.Memento();
            memento.Left = new SidePanel.Memento();
            memento.Right = new SidePanel.Memento();
            this.SidePanel.Restore(memento, _panels);
            this.SidePanelFrame.Reflesh();
        }

        private void RestoreMenu_Click(object sender, RoutedEventArgs e)
        {
            this.SidePanel.Restore(_memento, _panels);
            this.SidePanelFrame.Reflesh();
        }

        private int _brushCount;
        private void RandomBackground_Click(object sender, RoutedEventArgs e)
        {
            switch (++_brushCount % 3)
            {
                case 0:
                    this.BgBrush = Brushes.Black;
                    App.Current.Resources["PanelBrush"] = Brushes.Red;
                    App.Current.Resources["PageIconBrush"] = Brushes.Red;
                    break;
                case 1:
                    this.BgBrush = Brushes.White;
                    App.Current.Resources["PanelBrush"] = Brushes.Green;
                    App.Current.Resources["PageIconBrush"] = Brushes.Green;

                    break;
                case 2:
                    this.BgBrush = Brushes.Orange;
                    App.Current.Resources["PanelBrush"] = Brushes.Blue;
                    App.Current.Resources["PageIconBrush"] = Brushes.Blue;
                    break;
            }
        }


        /// <summary>
        /// CanvasWidth property.
        /// </summary>
        public double CanvasWidth
        {
            get { return _CanvasWidth; }
            set { if (_CanvasWidth != value) { _CanvasWidth = value; RaisePropertyChanged(); } }
        }

        private double _CanvasWidth;


        /// <summary>
        /// CanvasHeight property.
        /// </summary>
        public double CanvasHeight
        {
            get { return _CanvasHeight; }
            set { if (_CanvasHeight != value) { _CanvasHeight = value; RaisePropertyChanged(); } }
        }

        private double _CanvasHeight;


    }

}

