using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// SettingItemSusiePluginControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemSusiePluginControl : UserControl, INotifyPropertyChanged
    {
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion

        //
        public SettingItemSusiePluginControl()
        {
            InitializeComponent();
        }

        //
        public SettingItemSusiePluginControl(Susie.SusiePluginType pluginType)
        {
            InitializeComponent();
            this.Root.DataContext = SusieContext.Current.Susie;

            var binding = new Binding(pluginType == Susie.SusiePluginType.Image ? nameof(Susie.Susie.INPluginList) : nameof(Susie.Susie.AMPluginList));
            BindingOperations.SetBinding(this.PluginListBox, ListBox.ItemsSourceProperty, binding);
            BindingOperations.SetBinding(this.PluginListBox, ListBox.TagProperty, binding);
        }

        // プラグインリスト：ドロップ受付判定
        private void PluginListView_PreviewDragOver(object sender, DragEventArgs e)
        {
            ListBoxDragSortExtension.PreviewDragOver(sender, e, "SusiePlugin");
        }

        // プラグインリスト：ドロップ
        private void PluginListView_Drop(object sender, DragEventArgs e)
        {
            var list = (sender as ListBox).Tag as ObservableCollection<Susie.SusiePlugin>;
            if (list != null)
            {
                ListBoxDragSortExtension.Drop<Susie.SusiePlugin>(sender, e, "SusiePlugin", list);
            }
        }
    }
}
