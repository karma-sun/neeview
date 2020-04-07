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
using NeeLaboratory.ComponentModel;
using System.Windows.Markup;
using System.Net.Http;
using NeeView.Native;

namespace NeeView
{
    /// <summary>
    /// VersionWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionWindow : Window
    {
        private VersionWindowVM _vm;

        public VersionWindow()
        {
            Interop.NVFpReset();

            InitializeComponent();

            _vm = new VersionWindowVM();
            this.DataContext = _vm;
        }

        // from http://gushwell.ldblog.jp/archives/52279481.html
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.DialogHyperLinkFailedTitle).ShowDialog();
            }
        }
    }

    /// <summary>
    /// VersionWindow の ViewModel
    /// </summary>
    public class VersionWindowVM : BindableBase
    {
        public VersionWindowVM()
        {
            LicenseUri = "file://" + Environment.AssemblyLocation.Replace('\\', '/').TrimEnd('/') + $"/{Properties.Resources.HelpReadMeFile}";

            this.Icon = ResourceBitmapUtility.GetIconBitmapFrame("/Resources/App.ico", 256);

            // チェック開始
            Checker.CheckStart();
        }


        public string ApplicationName => Environment.ApplicationName;
        public string DispVersion => Environment.DispVersion + $" ({(Environment.IsX64 ? "64bit" : "32bit")})";
        public string LicenseUri { get; private set; }
        public string ProjectUri => "https://bitbucket.org/neelabo/neeview/";
        public bool IsNetworkEnabled => Config.Current.System.IsNetworkEnabled;
        public bool IsCheckerEnabled => Checker.IsEnabled;

        public BitmapFrame Icon { get; set; }

        // バージョンチェッカーは何度もチェックしないようにstaticで確保する
        public static VersionChecker Checker { get; set; } = new VersionChecker();
    }

    /// <summary>
    /// バージョンチェッカー
    /// </summary>
    public class VersionChecker : BindableBase
    {
        private bool _isCheching = false;
        private bool _isChecked = false;
        private string _message;


        public VersionChecker()
        {
            CurrentVersion = new FormatVersion(Environment.SolutionName);

#if DEBUG
            // for Debug
            //CurrentVersion = new FormatVersion(Environment.SolutionName, 36, 2, 0);
#endif
        }


#if DEBUG
        public string DownloadUri => "https://neelabo.bitbucket.io/NeeViewUpdateCheck.html";
#else
        public string DownloadUri => "https://bitbucket.org/neelabo/neeview/downloads";
#endif

        public bool IsEnabled => Config.Current.System.IsNetworkEnabled && !Environment.IsAppxPackage && !Environment.IsCanaryPackage && !Environment.IsBetaPackage;

        public FormatVersion CurrentVersion { get; set; }
        public FormatVersion LastVersion { get; set; }

        public bool IsExistNewVersion { get; set; }

        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(); }
        }


        public void CheckStart()
        {
            if (_isChecked || _isCheching) return;

            if (IsEnabled)
            {
                // チェック開始
                LastVersion = new FormatVersion(Environment.SolutionName, 0, 0, 0);
                Message = Properties.Resources.ControlAboutChecking;
                Task.Run(() => CheckVersion(Environment.IsZipLikePackage ? ".zip" : Environment.PackageType));
            }
        }

        private async Task CheckVersion(string extension)
        {
            _isCheching = true;

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(new Uri(DownloadUri));
                    response.EnsureSuccessStatusCode();
                    var text = await response.Content.ReadAsStringAsync();

#if DEBUG
                    ////extension = ".msi";
#endif

                    var regex = new Regex(@"NeeView(?<major>\d+)\.(?<minor>\d+)(?<arch>-[^\.]+)?" + Regex.Escape(extension));
                    var matches = regex.Matches(text);
                    if (matches.Count <= 0) throw new ApplicationException(Properties.Resources.ControlAboutWrongFormat);
                    foreach (Match match in matches)
                    {
                        var major = int.Parse(match.Groups["major"].Value);
                        var minor = int.Parse(match.Groups["minor"].Value);
                        var version = new FormatVersion(Environment.SolutionName, major, minor, 0);

                        Debug.WriteLine($"NeeView {major}.{minor} - {version:x8}: {match.Groups["arch"]?.Value}");
                        if (LastVersion.CompareTo(version) < 0)
                        {
                            LastVersion = version;
                        }
                    }

                    if (LastVersion == CurrentVersion)
                    {
                        Message = Properties.Resources.ControlAboutLastest;
                    }
                    else if (LastVersion.CompareTo(CurrentVersion) < 0)
                    {
                        Message = Properties.Resources.ControlAboutUnknown;
                    }
                    else
                    {
                        Message = Properties.Resources.ControlAboutNew;
                        IsExistNewVersion = true;
                        RaisePropertyChanged(nameof(IsExistNewVersion));
                    }

                    _isChecked = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Message = Properties.Resources.ControlAboutFailed;
            }

            _isCheching = false;
        }
    }
}
