using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NeeLaboratory.ComponentModel;
using System.Net.Http;

namespace NeeView
{
    /// <summary>
    /// バージョンチェッカー
    /// </summary>
    public class VersionChecker : BindableBase
    {
        private volatile bool _isCheching = false;
        private volatile bool _isChecked = false;
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
