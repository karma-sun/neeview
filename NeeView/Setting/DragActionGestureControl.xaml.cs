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

namespace NeeView.Setting
{
    /// <summary>
    /// DragActionGestureControl.xaml の相互作用ロジック
    /// </summary>
    public partial class DragActionGestureControl : UserControl
    {
        private DragActionGestureControlViewModel _vm;


        public DragActionGestureControl()
        {
            InitializeComponent();
        }


        internal void Initialize(DragActionCollection memento, string key)
        {
            _vm = new DragActionGestureControlViewModel(memento, key, this.GestureBox);
            DataContext = _vm;
        }

        internal void Decide()
        {
            _vm?.Decide();
        }
    }
}
