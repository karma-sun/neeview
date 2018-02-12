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
    /// HelpPopupControl.xaml の相互作用ロジック
    /// </summary>
    public partial class HelpPopupControl : UserControl
    {
        public UIElement PopupContent
        {
            get { return (UIElement)GetValue(PopupContentProperty); }
            set { SetValue(PopupContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupContentProperty =
            DependencyProperty.Register("PopupContent", typeof(UIElement), typeof(HelpPopupControl), new PropertyMetadata(null, PopupContent_Changed));

        private static void PopupContent_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is HelpPopupControl control)
            {
                control.PopupContentBorder.Child = control.PopupContent;
            }
        }

        public HelpPopupControl()
        {
            InitializeComponent();
        }
    }
}
