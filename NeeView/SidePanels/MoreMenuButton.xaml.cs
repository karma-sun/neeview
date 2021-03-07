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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{

    /// <summary>
    /// MoreMenu.xaml の相互作用ロジック
    /// </summary>
    public partial class MoreMenuButton : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
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


        private ContextMenu _moreMenu;


        public MoreMenuButton()
        {
            InitializeComponent();

            this.MoreButton.DataContext = this;
        }



        public MoreMenuDescription Description
        {
            get { return (MoreMenuDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(MoreMenuDescription), typeof(MoreMenuButton), new PropertyMetadata(null, DescriptionChanged));


        private static void DescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MoreMenuButton control)
            {
                control.Reset();
            }
        }


        public ContextMenu MoreMenu
        {
            get { return _moreMenu; }
            private set { SetProperty(ref _moreMenu, value); }
        }


        private void Reset()
        {
            MoreMenu = Description?.Create();
        }


        private void MoreButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MoreButton.IsChecked = !MoreButton.IsChecked;
            e.Handled = true;
        }

        private void MoreButton_Checked(object sender, RoutedEventArgs e)
        {
            if (Description != null)
            {
                MoreMenu = Description.Update(MoreMenu);
            }
            ContextMenuWatcher.SetTargetElement((UIElement)sender);
        }

    }


    public abstract class MoreMenuDescription
    {
        public abstract ContextMenu Create();

        public virtual ContextMenu Update(ContextMenu menu)
        {
            return menu;
        }


        protected MenuItem CreateCheckMenuItem(string header, Binding binding)
        {
            var item = new MenuItem();
            item.Header = header;
            item.IsCheckable = true;
            item.SetBinding(MenuItem.IsCheckedProperty, binding);
            return item;
        }

        protected MenuItem CreateCommandMenuItem(string header, ICommand command, Binding binding = null)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = command;
            if (binding != null)
            {
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }
            return item;
        }

        protected MenuItem CreateCommandMenuItem(string header, string command)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = RoutedCommandTable.Current.Commands[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            var binding = CommandTable.Current.GetElement(command).CreateIsCheckedBinding();
            if (binding != null)
            {
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }

            return item;
        }
    }
}
