using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// MenuBar : View
    /// </summary>
    public partial class MenuBarView : UserControl
    {
        #region Fields

        private MenuBarViewModel _vm;

        #endregion

        #region Constructors

        public MenuBarView()
        {
            InitializeComponent();

            this.CanaryWatermark.Visibility = Config.Current.IsCanaryPackage ? Visibility.Visible : Visibility.Collapsed;

#if DEBUG
            this.MenuBarArea.Children.Add(new DebugMenu());
#endif
        }

        #endregion

        #region DependencyProperties

        public MenuBar Source
        {
            get { return (MenuBar)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MenuBar), typeof(MenuBarView), new PropertyMetadata(null, Source_Changed));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MenuBarView)?.Initialize();
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            _vm = new MenuBarViewModel(this, this.Source);
            this.Root.DataContext = _vm;
        }

        // 単キーのショートカット無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        #endregion
    }
}
