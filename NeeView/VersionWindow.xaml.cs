// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NeeView
{
    /// <summary>
    /// VersionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionWindow : Window
    {
        VersionWindowVM _VM;

        public VersionWindow()
        {
            InitializeComponent();

            _VM = new VersionWindowVM();
            this.DataContext = _VM;
        }

        // from http://gushwell.ldblog.jp/archives/52279481.html
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }


    // コンバータ：バージョン番号
    [ValueConversion(typeof(int), typeof(string))]
    public class VersionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int? version = value as int?;
            if (version != null)
            {
                return $"1.{version}";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// VersionWindow の ViewModel
    /// </summary>
    public class VersionWindowVM : INotifyPropertyChanged
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

        public string LicenseUri { get; private set; }
        public string ProjectUri => "https://bitbucket.org/neelabo/neeview/";
        public string ChangeLogUri => "https://bitbucket.org/neelabo/neeview/wiki/ChangeLog";

        // バージョンチェッカーは何度もチェックしないようにstaticで確保する
        public static VersionChecker Checker { get; set; } = new VersionChecker();

        //
        public VersionWindowVM()
        {
            LicenseUri = "file://" + Environment.CurrentDirectory.Replace('\\', '/').TrimEnd('/') + "/README.html#license";

            // チェック開始
            Checker.CheckStart();
        }

    }

    /// <summary>
    /// バージョンチェッカー
    /// </summary>
    public class VersionChecker : INotifyPropertyChanged
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

        public string DownloadUri => "https://bitbucket.org/neelabo/neeview/downloads";

        public int CurrentVersion { get; set; }
        public int LastVersion { get; set; }

        public bool IsExistNewVersion { get; set; }

        #region Property: Message
        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { _Message = value; OnPropertyChanged(); }
        }
        #endregion


        private bool _IsCheching = false;
        private bool _IsChecked = false;

        //
        public VersionChecker()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = FileVersionInfo.GetVersionInfo(assembly.Location);
            CurrentVersion = ver.FileMinorPart;

#if DEBUG
            // for Debug
            //CurrentVersion = 5;
#endif
        }

        //
        public void CheckStart()
        {
            if (_IsChecked || _IsCheching) return;

            // チェック開始
            LastVersion = 0; // CurrentVersion;
            Message = "最新バージョンをチェック中...";
            Task.Run(CheckVersion);
        }


        private async Task CheckVersion()
        {
            _IsCheching = true;

            try
            {
                using (var wc = new System.Net.WebClient())
                {
                    // download
                    var text = await wc.DownloadStringTaskAsync(new Uri(DownloadUri));

                    var regex = new Regex(@"NeeView1\.(\d+)\.zip");
                    var matches = regex.Matches(text);
                    if (matches.Count <= 0) throw new ApplicationException("更新ページのフォーマットが想定されているものと異なります");
                    foreach (Match match in matches)
                    {
                        var version = int.Parse(match.Groups[1].Value);
                        Debug.WriteLine("NeeView 1.{0}", version);
                        if (LastVersion < version) LastVersion = version;
                    }

                    if (LastVersion == CurrentVersion)
                    {
                        Message = "NeeView は最新のバージョンです";
                    }
                    else if (LastVersion < CurrentVersion)
                    {
                        Message = "NeeView は未知のバージョンです";
                    }
                    else
                    {
                        Message = $"新しいバージョンがリリースされています";
                        IsExistNewVersion = true;
                        OnPropertyChanged(nameof(IsExistNewVersion));
                    }

                    _IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Message = "更新チェックに失敗しました";
            }

            _IsCheching = false;
        }
    }
}
