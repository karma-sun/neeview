using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class ExternalProcessOptions
    {
        public bool IsThrowException { get; set; }
        public string WorkingDirectory { get; set; }
    }

    public static class ExternalProcess
    {
        private static Regex _httpPrefix = new Regex(@"^\s*http[s]?:", RegexOptions.IgnoreCase);
        private static Regex _htmlPostfix = new Regex(@"\.htm[l]?$", RegexOptions.IgnoreCase);

        public static void Start(string filename, string args = null, ExternalProcessOptions options = null)
        {
            options = options ?? new ExternalProcessOptions();

            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = options.WorkingDirectory ?? startInfo.WorkingDirectory;

            if (string.IsNullOrWhiteSpace(filename))
            {
                startInfo.FileName = args;
            }
            else
            {
                startInfo.FileName = filename;
                startInfo.Arguments = args;
            }

            if (string.IsNullOrWhiteSpace(startInfo.FileName))
            {
                return;
            }

            if (!Config.Current.System.IsNetworkEnabled && _httpPrefix.IsMatch(startInfo.FileName))
            {
                var dialog = new MessageDialog(Properties.Resources.ExternalProcess_ConfirmBrowserDialog_Message, Properties.Resources.ExternalProcess_ConfirmBrowserDialog_Title);
                dialog.Commands.AddRange(UICommands.OKCancel);
                var result = dialog.ShowDialog();
                if (result?.IsPositibe != true)
                {
                    return;
                }
            }

            if (Config.Current.System.WebBrowser != null && string.IsNullOrEmpty(startInfo.Arguments) && IsBrowserContent(startInfo.FileName))
            {
                startInfo.Arguments = startInfo.FileName;
                startInfo.FileName = Config.Current.System.WebBrowser;
            }

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                if (options.IsThrowException)
                {
                    throw;
                }
                else
                {

                    ToastService.Current.Show(new Toast(ex.Message + "\r\n\r\n" + startInfo.FileName, Properties.Resources.Word_Error, ToastIcon.Error));
                }
            }
        }


        public static void OpenWithTextEditor(string path)
        {
            var textEditor = Config.Current.System.TextEditor ?? "notepad.exe";
            Start(textEditor, $"\"{path}\"");
        }


        private static bool IsBrowserContent(string path)
        {
            var isResult = _httpPrefix.IsMatch(path) || _htmlPostfix.IsMatch(path);
            return isResult;
        }
    }
}
