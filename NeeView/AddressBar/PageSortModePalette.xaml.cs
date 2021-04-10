using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// PageSortModePalette.xaml の相互作用ロジック
    /// </summary>
    public partial class PageSortModePalette : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        private PageSortMdePaletteViewModel _vm;

        public PageSortModePalette()
        {
            InitializeComponent();

            _vm = new PageSortMdePaletteViewModel();
            this.Root.DataContext = _vm;

            this.Loaded += (s, e) => this.Items.Focus();
        }


        public event EventHandler SelfClosed;


        public Popup ParentPopup
        {
            get { return (Popup)GetValue(ParentPopupProperty); }
            set { SetValue(ParentPopupProperty, value); }
        }

        public static readonly DependencyProperty ParentPopupProperty =
            DependencyProperty.Register("ParentPopup", typeof(Popup), typeof(PageSortModePalette), new PropertyMetadata(null));


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var select = (PageSortMode)((Button)sender).Tag;
            _vm.Decide(select);
            Close();
        }

        private void Root_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (Keyboard.Modifiers == ModifierKeys.None)
                    {
                        Close();
                        e.Handled = true;
                    }
                    break;

                case Key.Left:
                    MoveFocus(FocusNavigationDirection.Left);
                    e.Handled = true;
                    break;

                case Key.Up:
                    MoveFocus(FocusNavigationDirection.Previous);
                    e.Handled = true;
                    break;

                case Key.Right:
                    MoveFocus(FocusNavigationDirection.Right);
                    e.Handled = true;
                    break;

                case Key.Down:
                    MoveFocus(FocusNavigationDirection.Next);
                    e.Handled = true;
                    break;
            }
        }

        private void Close()
        {
            SelfClosed?.Invoke(this, null);

            if (ParentPopup != null)
            {
                ParentPopup.IsOpen = false;
            }
        }

        private void MoveFocus(FocusNavigationDirection direction)
        {
            var element = FocusManager.GetFocusedElement(Window.GetWindow(this)) as UIElement;
            if (element == null)
            {
                element = this.Items;
            }

            element.MoveFocus(new TraversalRequest(direction));
        }
    }

    public class PageSortMdePaletteViewModel : BindableBase
    {
        public List<PageSortMode> PageSortModeList => Enum.GetValues(typeof(PageSortMode)).Cast<PageSortMode>().ToList();


        public void Decide(PageSortMode mode)
        {
            BookSettingPresenter.Current.SetSortMode(mode);
        }
    }

}
