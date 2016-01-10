// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
            set
            {
                _SusiePluginPath = value;
                Setting.SusieMemento.SusiePluginPath = value;
                UpdateSusiePluginSetting();
            }
        }
        #endregion


        // 設定
        public Setting Setting { get; set; }

        // Susieプラグインリスト
        public ObservableCollection<Susie.SusiePlugin> SusiePluginList { get; private set; } = new ObservableCollection<Susie.SusiePlugin>();


        // コマンド一覧用パラメータ
        public class CommandParam
        {
            public CommandType Key { get; set; }
            public string Group { get; set; }
            public string Header { get; set; }
            public string ShortCut { get; set; }
            public string MouseGesture { get; set; }
            public bool IsShowMessage { get; set; }
        }

        // コマンド一覧
        public ObservableCollection<CommandParam> CommandCollection { get; set; }


        // Susieプラグイン コンフィグコマンド
        public static readonly RoutedCommand SusiePluginConfigCommand = new RoutedCommand("SusiePluginConfigCommand", typeof(SettingWindow));
 

        // 背景タイプリスト
        public static Dictionary<BackgroundStyle, string> BackgroundStyleList { get; } = new Dictionary<BackgroundStyle, string>
        {
            [BackgroundStyle.Black] = "黒色",
            [BackgroundStyle.White] = "白色",
            [BackgroundStyle.Auto] = "画像に合わせた色",
            [BackgroundStyle.Check] = "チェック模様",
        };

        // 通知表示タイプリスト
        public static Dictionary<ShowMessageStyle, string> ShowMessageTypeList { get; } = new Dictionary<ShowMessageStyle, string>
        {
            [ShowMessageStyle.None] = "表示しない",
            [ShowMessageStyle.Normal] = "表示する",
            [ShowMessageStyle.Tiny] = "小さく表示する",
        };

        // 履歴MAXリスト
        public static List<int> MaxHistoryCountList { get; } = new List<int>
        {
            10,
            100,
            1000,
            10000
        };

        // スライドショー切り替え時間リスト
        public static List<double> SlideShowIntervalList { get; } = new List<double>
        {
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            15,
            20,
            30,
            60,
        };



        public SettingWindow(Setting setting)
        {
            InitializeComponent();

            Setting = setting;
            
            // コマンド一覧作成
            CommandCollection = new ObservableCollection<CommandParam>();
            UpdateCommandList();

            // プラグイン一覧作成
            _SusiePluginPath = Setting.SusieMemento.SusiePluginPath;
            UpdateSusiePluginList();
            
            // 自身をコンテキストにする
            this.DataContext = this;

            // コマンド設定
            this.PluginListView.CommandBindings.Add(new CommandBinding(SusiePluginConfigCommand, SusiePluginConfigCommand_Executed));

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }


        // コマンド一覧 更新
        private void UpdateCommandList()
        {
            CommandCollection.Clear();
            foreach (var element in ModelContext.CommandTable)
            {
                var item = new CommandParam()
                {
                    Key = element.Key,
                    Group = element.Value.Group,
                    Header = element.Value.Text,
                    ShortCut = Setting.CommandMememto[element.Key].ShortCutKey,
                    MouseGesture = Setting.CommandMememto[element.Key].MouseGesture,
                    IsShowMessage = Setting.CommandMememto[element.Key].IsShowMessage,
                };
                CommandCollection.Add(item);
            }
        }

        // Susieプラグイン コンフィグ実行
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

        // Susie環境 更新
        private void UpdateSusiePluginSetting()
        {
            ModelContext.SusieContext.Restore(Setting.SusieMemento);
            UpdateSusiePluginList();
        }

        // Susieプラグイン一覧 更新
        public void UpdateSusiePluginList()
        {
            SusiePluginList.Clear();

            foreach (var plugin in ModelContext.Susie.PluginCollection)
            {
                SusiePluginList.Add(plugin);
            }
        }

        // 決定ボタン処理
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // コマンド設定反映
            foreach (var command in CommandCollection)
            {
                Setting.CommandMememto[command.Key].ShortCutKey = command.ShortCut;
                Setting.CommandMememto[command.Key].MouseGesture = command.MouseGesture;
                Setting.CommandMememto[command.Key].IsShowMessage = command.IsShowMessage;
            }

            // Susie設定反映
            Setting.SusieMemento.SetSpiFiles(ModelContext.Susie);

            this.DialogResult = true;
            this.Close();
        }

        // キャンセルボタン処理
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ショートカットキー設定ボタン処理
        private void ShortCutSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (CommandParam)this.CommandListView.SelectedValue;

            var dialog = new InputGestureSettingWindow(value);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                this.CommandListView.Items.Refresh();
            }
        }

        // マウスジェスチャー設定ボタン処理
        private void MouseGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (CommandParam)this.CommandListView.SelectedValue;

            var dialog = new MouseGestureSettingWindow(value);
            dialog.Owner = this;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                this.CommandListView.Items.Refresh();
            }
        }

        // キーバインド初期化ボタン処理
        private void ResetGestureSettingButton_Click(object sender, RoutedEventArgs e)
        {
            Setting.CommandMememto = CommandTable.CreateDefaultMemento();
            UpdateCommandList();
            this.CommandListView.Items.Refresh();
        }

        // 履歴クリアボタン処理
        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            Setting.BookHistoryMemento.History.Clear();
            OnPropertyChanged(nameof(Setting));
        }
    }


    // プラグイングループ分け用
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

    // ジェスチャー表示用コンバータ
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
