using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NeeView
{
    [Flags]
    public enum ExternalProcessAtrtibute
    {
        None = 0,
        ThrowException = (1<<0),
    }

    public static class ExternalProcess
    {
        private static Regex _httpPrefix = new Regex(@"^\s*http[s]?:");

        public static void Start(string filename, string args = null, ExternalProcessAtrtibute attributes = ExternalProcessAtrtibute.None)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;

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

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                if ((attributes & ExternalProcessAtrtibute.ThrowException) == ExternalProcessAtrtibute.ThrowException)
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
    }
}
