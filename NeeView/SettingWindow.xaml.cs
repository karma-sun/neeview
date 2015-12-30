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
using System.Windows.Shapes;

using System.Collections.ObjectModel;

namespace NeeView
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window, INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        #region Property: SusiePluginPath
        private string _SusiePluginPath;
        public string SusiePluginPath
        {
            get { return _SusiePluginPath; }
            set { _SusiePluginPath = value; Setting.SusieSetting.SusiePluginPath = value; UpdateSusiePluginSetting(); }
        }
        #endregion

        public Setting Setting { get; set; }

        public ObservableCollection<Susie.SusiePlugin> SusiePluginList { get; private set; } = new ObservableCollection<Susie.SusiePlugin>();

        public static readonly RoutedCommand SusiePluginConfigCommand = new RoutedCommand("SusiePluginConfigCommand", typeof(MainWindow));
        public static readonly RoutedCommand SusiePluginUpdateCommand = new RoutedCommand("SusiePluginUpdateCommand", typeof(MainWindow));

        public static BackgroundStyle[] BackgroundStyleEnum { get; } = (BackgroundStyle[])Enum.GetValues(typeof(BackgroundStyle));

        public static Dictionary<BackgroundStyle, string> BackgroundStyleList { get; } = new Dictionary<BackgroundStyle, string>
        {
            [BackgroundStyle.Black] = "黒色",
            [BackgroundStyle.White] = "白色",
            [BackgroundStyle.Auto] = "自動",
            [BackgroundStyle.Check] = "チェック模様",
        };

        public static Dictionary<ShowMessageType, string> ShowMessageTypeList { get; } = new Dictionary<ShowMessageType, string>
        {
            [ShowMessageType.None] = "表示しない",
            [ShowMessageType.Normal] = "表示する",
            [ShowMessageType.Tiny] = "小さく表示する",
        };

        public static List<int> MaxHistoryCountList { get; } = new List<int>
        {
            10,
            100,
            1000,
            10000
        };

        public class BookCommand
        {
            public BookCommandType Key { get; set; }
            public string Group { get; set; }
            public string Header { get; set; }
            public string ShortCut { get; set; }
            public string MouseGesture { get; set; }
            public bool IsShowMessage { get; set; }
        }

        public ObservableCollection<BookCommand> BookCommandCollection { get; set; }

        public MainWindowVM VM { get; set; }

        public SettingWindow(MainWindowVM  vm, Setting setting)
        {
            VM = vm;
            Setting = setting;

            _SusiePluginPath = Setting.SusieSetting.SusiePluginPath;

            BookCommandCollection = new ObservableCollection<BookCommand>();
            CreateCommandList();

            InitializeComponent();

            this.DataContext = this;

            this.PluginListView.CommandBindings.Add(new CommandBinding(SusiePluginConfigCommand, SusiePluginConfigCommand_Executed));
            this.SusieSettingTab.CommandBindings.Add(new CommandBinding(SusiePluginUpdateCommand, SusiePluginUpdateCommand_Executed));

            UpdateSusiePluginList();
        }


        private void CreateCommandList()
        {
            BookCommandCollection.Clear();
            foreach (var header in BookCommandExtension.Headers)
            {
                var item = new BookCommand()
                {
                    Key = header.Key,
                    Group = header.Value.Group,
                    Header = header.Value.Text,
                    ShortCut = Setting.GestureSetting[header.Key].ShortCutKey,
                    MouseGesture = Setting.GestureSetting[header.Key].MouseGesture,
                    IsShowMessage = Setting.GestureSetting[header.Key].IsShowMessage,
                };
                BookCommandCollection.Add(item);
            }
        }

    private void SusiePluginConfigCommand_Executed(object source, ExecutedRoutedEventArgs e)
        {
            var plugin = (Susie.SusiePlugin)e.Parameter;

            int result = plugin.ConfigurationDlg(this);

            // 設定ウィンドウが呼び出せなかった場合はアバウト画面でお茶を濁す
            if (result < 0)
            {
                plugin.AboutDlg(this);
            }
        }

        private void SusiePluginUpdateCommand_Executed(object source, ExecutedRoutedEventArgs e)
        {
            Setting.SusieSetting.Restore(ModelContext.SusieContext);
            UpdateSusiePluginList();
        }

        private void UpdateSusiePluginSetting()
        {
            Setting.SusieSetting.Restore(ModelContext.SusieContext);
            UpdateSusiePluginList();
        }

        public void UpdateSusiePluginList()
        {
            SusiePluginList.Clear();
            ModelContext.Susie.AMPlgunList.ForEach(e => SusiePluginList.Add(e));
            ModelContext.Susie.INPlgunList.ForEach(e => SusiePluginList.Add(e));
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var command in BookCommandCollection)
            {
                Setting.GestureSetting[command.Key].ShortCutKey = command.ShortCut;
                Setting.GestureSetting[command.Key].MouseGesture = command.MouseGesture;
                Setting.GestureSetting[command.Key].IsShowMessage = command.IsShowMessage;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShortCutSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (BookCommand)this.CommandListView.SelectedValue;

            var dialog = new InputGestureSettingWindow(value);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                this.CommandListView.Items.Refresh();
            }
        }

        private void MouseGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (BookCommand)this.CommandListView.SelectedValue;

            var dialog = new MouseGestureSettingWindow(value);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                this.CommandListView.Items.Refresh();
            }
        }

        private void ResetGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            Setting.GestureSetting = BookCommandShortcutSource.CreateDefaultShortcutSource();
            CreateCommandList();
            this.CommandListView.Items.Refresh();
        }

        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            //VM.BookCommands[BookCommandType.ClearHistory].Execute(this, null);
            //VM.ClearHistor();
            Setting.BookHistory.Clear();
            OnPropertyChanged("Setting");
        }
    }



    [ValueConversion(typeof(string), typeof(string))]
    public class ApiVersionToApiTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((string)value)
            {
                case "00AM":
                    return "圧縮ファイル展開用プラグイン";
                case "00IN":
                    return "画像表示用プラグイン";
                default:
                    return "その他";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [ValueConversion(typeof(string), typeof(string))]
    public class MouseGestureToDispTextConverter : IValueConverter
    {
        private static Dictionary<char, char> _Table = new Dictionary<char, char>
        {
            ['U'] = '↑',
            ['R'] = '→',
            ['D'] = '↓',
            ['L'] = '←',
        };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            string text = "";
            foreach (char c in (string)value)
            {
                text += _Table[c];
            }
            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
