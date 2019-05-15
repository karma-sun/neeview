using NeeLaboratory.Windows.Input;
using NeeView.Susie;
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

        #region Fields

        private SusiePluginType _pluginType;

        #endregion

        #region Constructors

        public SettingItemSusiePluginControl()
        {
            InitializeComponent();
        }

        public SettingItemSusiePluginControl(SusiePluginType pluginType)
        {
            InitializeComponent();

            this.Root.DataContext = this;

            _pluginType = pluginType;

            Plugins = new ObservableCollection<SusiePluginInfo>(SusieContext.Current.Client?.GetPlugins(null).Where(e => e.PluginType == pluginType));
        }

        #endregion


        private ObservableCollection<SusiePluginInfo> _plugins;
        public ObservableCollection<SusiePluginInfo> Plugins
        {
            get { return _plugins; }
            set { SetProperty(ref _plugins, value); }
        }


        #region Commands

        private RelayCommand _configCommand;
        public RelayCommand ConfigCommand
        {
            get { return _configCommand = _configCommand ?? new RelayCommand(OpenConfigDialog_Executed, CanOpenConfigDialog); }
        }

        private bool CanOpenConfigDialog()
        {
            var item = this.PluginList.SelectedItem as SusiePluginInfo;
            return item != null;
        }

        private void OpenConfigDialog_Executed()
        {
            var item = this.PluginList.SelectedItem as SusiePluginInfo;
            OpenConfigDialog(item);
        }

        private void OpenConfigDialog(SusiePluginInfo spi)
        {
            if (spi == null) return;

            var dialog = new SusiePluginSettingWindow(spi);
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();

            // TODO: 情報更新
            Debug.WriteLine($"TODO: imprement RaiseDetailTextPropertyChanged()");
            UpdateExtensions();
        }


        private RelayCommand _moveUpCommand;
        public RelayCommand MoveUpCommand
        {
            get { return _moveUpCommand = _moveUpCommand ?? new RelayCommand(MoveUpCommand_Executed); }
        }

        private void MoveUpCommand_Executed()
        {
            var index = this.PluginList.SelectedIndex;
            var collection = this.PluginList.Tag as ObservableCollection<SusiePluginInfo>;
            if (index > 0)
            {
                collection.Move(index, index - 1);
                this.PluginList.ScrollIntoView(this.PluginList.SelectedItem);
            }
        }

        private RelayCommand _moveDownCommand;
        public RelayCommand MoveDownCommand
        {
            get { return _moveDownCommand = _moveDownCommand ?? new RelayCommand(MoveDownCommand_Executed); }
        }

        private void MoveDownCommand_Executed()
        {
            var index = this.PluginList.SelectedIndex;
            var collection = this.PluginList.Tag as ObservableCollection<SusiePluginInfo>;
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
            var list = (sender as ListBox).Tag as ObservableCollection<SusiePluginInfo>;
            if (list != null)
            {
                ListBoxDragSortExtension.Drop<SusiePluginInfo>(sender, e, "SusiePlugin", list);
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
            var item = (sender as ListBoxItem)?.DataContext as SusiePluginInfo;
            OpenConfigDialog(item);
        }

        // 有効/無効チェックボックス
        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateExtensions();
        }

        private void UpdateExtensions()
        {
            Debug.WriteLine($"TODO: Update plugin extensions");
            if (_pluginType == SusiePluginType.Image)
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
