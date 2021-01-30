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

namespace NeeView
{
    /// <summary>
    /// ManViewBay.xaml の相互作用ロジック
    /// </summary>
    public partial class MainViewBay : UserControl, IHasDeviceInput
    {
        private MouseInput _mouseInput;
        private TouchInput _touchInput;


        public MouseInput MouseInput => _mouseInput;
        public TouchInput TouchInput => _touchInput;

        public ThemeBrushProvider ThemeBrush => ThemeBrushProvider.Current;

        public MainViewBay()
        {
            InitializeComponent();

            this.DataContext = this;

            // mouse / touch command gesture binding
            var mouseGestureCommandCollection = MouseGestureCommandCollection.Current;
            _touchInput = new TouchInput(new TouchInputContext(this, null, mouseGestureCommandCollection, null, null, null));
            _mouseInput = new MouseInput(new MouseInputContext(this, mouseGestureCommandCollection, null, null, null));
            RoutedCommandTable.Current.AddMouseInput(_mouseInput);
            RoutedCommandTable.Current.AddTouchInput(_touchInput);

            // Drag&Drop設定
            this.AllowDrop = true;
            ContentDropManager.Current.SetDragDropEvent(this);
        }
    }
}
