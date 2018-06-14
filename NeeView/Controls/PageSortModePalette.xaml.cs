using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        }


        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(PageSortModePalette), new PropertyMetadata(false));


        private void ListBoxItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBoxItem)?.Content is PageSortMode select)
            {
                _vm.Decide(select);
                this.IsOpen = false;
            }
        }
    }

    public class PageSortMdePaletteViewModel : BindableBase
    {
        public List<PageSortMode> PageSortModeList => Enum.GetValues(typeof(PageSortMode)).Cast<PageSortMode>().ToList();


        public void Decide(PageSortMode mode)
        {
            BookSetting.Current.SetSortMode(mode);
        }
    }
}
