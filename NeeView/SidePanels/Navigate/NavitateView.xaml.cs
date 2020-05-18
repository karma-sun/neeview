using NeeView.Effects;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
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
    public partial class NavitateView : UserControl
    {
        private NavigateViewModel _vm;
        private bool _isFocusRequest;


        public NavitateView()
        {
            InitializeComponent();
        }

        public NavitateView(NavigateModel model) : this()
        {
            InitializeComponent();

            _vm = new NavigateViewModel(model);
            this.DataContext = _vm;

            this.IsVisibleChanged += NavigateView_IsVisibleChanged;
        }


        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        private void NavigateView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_isFocusRequest && this.IsVisible)
            {
                this.Focus();
                _isFocusRequest = false;
            }
        }

        private void BaseScale_ValueDelta(object sender, ValueDeltaEventArgs e)
        {
            _vm.AddBaseScaleTick(e.Delta);
        }

        private void Scale_ValueDelta(object sender, ValueDeltaEventArgs e)
        {
            _vm.AddScaleTick(e.Delta);
        }

        private void Angle_ValueDelta(object sender, ValueDeltaEventArgs e)
        {
            _vm.AddAngleTick(e.Delta);
        }


        public void FocusAtOnce()
        {
            var focused = this.Focus();
            if (!focused)
            {
                _isFocusRequest = true;
            }
        }

    }
}
