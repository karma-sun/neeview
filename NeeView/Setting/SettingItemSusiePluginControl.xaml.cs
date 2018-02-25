using NeeLaboratory.Windows.Input;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        #region Fields

        private Susie.SusiePluginType _pluginType;

        #endregion

        #region Constructors

        //
        public SettingItemSusiePluginControl()
        {
            InitializeComponent();
        }

        //
        public SettingItemSusiePluginControl(Susie.SusiePluginType pluginType)
        {
            InitializeComponent();
            this.Root.DataContext = this;

            _pluginType = pluginType;

            var binding = new Binding(pluginType == Susie.SusiePluginType.Image ? nameof(Susie.Susie.INPluginList) : nameof(Susie.Susie.AMPluginList))
            {
                Source = SusieContext.Current.Susie
            };
            BindingOperations.SetBinding(this.PluginList, ListBox.ItemsSourceProperty, binding);
            BindingOperations.SetBinding(this.PluginList, ListBox.TagProperty, binding);
        }

        #endregion
        
        #region Commands

        //
        private RelayCommand _configCommand;
        public RelayCommand ConfigCommand
        {
            get { return _configCommand = _configCommand ?? new RelayCommand(OpemConfigDialog_Executed, CanOpenConfigDialog); }
        }

        private bool CanOpenConfigDialog()
        {
            var item = this.PluginList.SelectedItem as Susie.SusiePlugin;
            return item != null ? item.HasConfigurationDlg : false;
        }

        private void OpemConfigDialog_Executed()
        {
            var item = this.PluginList.SelectedItem as Susie.SusiePlugin;
            if (item != null && item.HasConfigurationDlg)
            {
                item.OpenConfigurationDlg_Executed(Window.GetWindow(this));
            }
        }


        //
        private RelayCommand _moveUpCommand;
        public RelayCommand MoveUpCommand
        {
            get { return _moveUpCommand = _moveUpCommand ?? new RelayCommand(MoveUpCommand_Executed); }
        }

        private void MoveUpCommand_Executed()
        {
            var index = this.PluginList.SelectedIndex;
            var collection = this.PluginList.Tag as ObservableCollection<Susie.SusiePlugin>;
            if (index > 0)
            {
                collection.Move(index, index - 1);
                this.PluginList.ScrollIntoView(this.PluginList.SelectedItem);
            }
        }

        //
        private RelayCommand _moveDownCommand;
        public RelayCommand MoveDownCommand
        {
            get { return _moveDownCommand = _moveDownCommand ?? new RelayCommand(MoveDownCommand_Executed); }
        }

        private void MoveDownCommand_Executed()
        {
            var index = this.PluginList.SelectedIndex;
            var collection = this.PluginList.Tag as ObservableCollection<Susie.SusiePlugin>;
            if (index >= 0 && index < collection.Count - 1)
            {
                collection.Move(index, index + 1);
                this.PluginList.ScrollIntoView(this.PluginList.SelectedItem);
            }
        }

        #endregion

        #region Methods

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


        // 選択項目変更
        private void PluginList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ConfigCommand.RaiseCanExecuteChanged();
        }

        // 項目ダブルクリック
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListBoxItem)?.DataContext as Susie.SusiePlugin;
            if (item != null && item.HasConfigurationDlg)
            {
                item.OpenConfigurationDlg_Executed(Window.GetWindow(this));
            }
        }

        // 有効/無効チェックボックス
        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            // 対応拡張子の更新
            if (_pluginType == Susie.SusiePluginType.Image)
            {
                SusieContext.Current.UpdateImageExtensions();
            }
            else
            {
                SusieContext.Current.UpdateArchiveExtensions();
            }
        }

        #endregion
    }
}
