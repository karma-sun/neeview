using Microsoft.Win32;

namespace NeeView.Susie
{
    public static class SusieUtility
    {
        private static bool _susiePluginInstallPathInitialized;
        private static string _susiePluginInstallPath;

        // レジストリに登録されているSusiePluginパスの取得
        public static string GetSusiePluginInstallPath()
        {
            if (!_susiePluginInstallPathInitialized)
            {
                try
                {
                    RegistryKey regkey = Registry.CurrentUser.OpenSubKey(@"Software\Takechin\Susie\Plug-in", false);
                    _susiePluginInstallPath = (string)regkey?.GetValue("Path") ?? "";
                }
                catch
                {
                }
                _susiePluginInstallPathInitialized = true;
            }

            return _susiePluginInstallPath;
        }
    }
}
