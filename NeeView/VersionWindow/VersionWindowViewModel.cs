using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// VersionWindow の ViewModel
    /// </summary>
    public class VersionWindowViewModel : BindableBase
    {
        public VersionWindowViewModel()
        {
            LicenseUri = "file://" + Environment.AssemblyFolder.Replace('\\', '/').TrimEnd('/') + $"/{Properties.Resources.HelpReadMe_File}";

            this.Icon = ResourceBitmapUtility.GetIconBitmapFrame("/Resources/App.ico", 256);

            // チェック開始
            Checker.CheckStart();
        }


        public string ApplicationName => Environment.ApplicationName;
        public string DispVersion => Environment.DispVersion + $" ({(Environment.IsX64 ? "64bit" : "32bit")})";
        public string LicenseUri { get; private set; }
        public string ProjectUri => "https://bitbucket.org/neelabo/neeview/";
        public bool IsCheckerEnabled => Checker.IsEnabled;

        public BitmapFrame Icon { get; set; }

        // バージョンチェッカーは何度もチェックしないようにstaticで確保する
        public static VersionChecker Checker { get; set; } = new VersionChecker();


        public void CopyVersionToClipboard()
        {
            var s = new StringBuilder();
            s.AppendLine($"Version: {ApplicationName} {DispVersion}");
            s.AppendLine($"Package: {Environment.PackageType.TrimStart('.')}");
            s.AppendLine($"OS: {System.Environment.OSVersion}");

            Debug.WriteLine(s);

            Clipboard.SetText(s.ToString());
        }

    }
}
