using NeeView.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public enum SwitchOption
    {
        off,
        on,
    }

    public enum WindowStateOption
    {
        normal,
        min,
        max,
        full
    }

    public static class WindowStateOptionExtensions
    {
        public static WindowStateEx ToWindowStateEx(this WindowStateOption? option)
        {
            switch (option)
            {
                default: return WindowStateEx.None;
                case WindowStateOption.normal: return WindowStateEx.Normal;
                case WindowStateOption.min: return WindowStateEx.Minimized;
                case WindowStateOption.max: return WindowStateEx.Maximized;
                case WindowStateOption.full: return WindowStateEx.FullScreen;
            }
        }

    }

    public class CommandLineOption
    {
        [OptionMember("h", "help", Default = "true", HelpText = "@OptionIsHelp")]
        public bool IsHelp { get; set; }

        [OptionMember("v", "version", Default = "true", HelpText = "@OptionIsVersion")]
        public bool IsVersion { get; set; }

        [OptionMember("x", "setting", HasParameter = true, RequireParameter = true, HelpText = "@OptionSettingFilename")]
        public string SettingFilename { get; set; }

        [OptionMember("b", "blank", Default = "on", HelpText = "@OptionIsBlank")]
        public SwitchOption IsBlank { get; set; }

        [OptionMember("r", "reset-placement", Default = "on", HelpText = "@OptionIsResetPlacement")]
        public SwitchOption IsResetPlacement { get; set; }

        [OptionMember("n", "new-window", Default = "on", HasParameter = true, HelpText = "@OptionIsNewWindow")]
        public SwitchOption? IsNewWindow { get; set; }

        [OptionMember("s", "slideshow", Default = "on", HasParameter = true, HelpText = "@OptionIsSlideShow")]
        public SwitchOption? IsSlideShow { get; set; }

        [OptionMember("o", "folderlist", HasParameter = true, RequireParameter = true, HelpText = "@OptionFolderList")]
        public string FolderList { get; set; }

        [OptionMember(null, "window", HasParameter = true, RequireParameter = true, HelpText = "@OptionWindowState")]
        public WindowStateOption? WindowState { get; set; }

        [OptionMember(null, "script", HasParameter = true, RequireParameter = true, HelpText = "@OptionScriptFile")]
        public string ScriptFile { get; set; }


        [OptionValues]
        public List<string> Values { get; set; }


        public void Validate()
        {
            try
            {
                if (this.SettingFilename != null)
                {
                    var filename = this.SettingFilename;
                    if (File.Exists(filename))
                    {
                        // 念のためフルパス変換
                        this.SettingFilename = Path.GetFullPath(filename);
                    }
                    else
                    {
                        throw new ArgumentException($"{Properties.Resources.OptionErrorFileNotFound}: {this.SettingFilename}");
                    }
                }
                else
                {
                    this.SettingFilename = Path.Combine(Environment.LocalApplicationDataPath, SaveData.UserSettingFileName);
                }
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.DialogBootErrorTitle).ShowDialog();
                throw new OperationCanceledException("Wrong startup parameter");
            }

        }
    }


    public partial class App
    {
        public CommandLineOption ParseArguments(string[] args)
        {
            var optionMap = new OptionMap<CommandLineOption>();
            CommandLineOption option;

            try
            {
                var items = new List<string>(args);

                // プロトコル起動を吸収
                const string scheme = "neeview-open:";
                if (items.Any() && items[0].StartsWith(scheme))
                {
                    items[0] = items[0].Replace(scheme, "");
                    if (string.IsNullOrWhiteSpace(items[0]))
                    {
                        items.RemoveAt(0);
                    }
                }

                option = optionMap.ParseArguments(items.ToArray());
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, NeeView.Properties.Resources.DialogBootErrorTitle).ShowDialog();
                throw new OperationCanceledException("Wrong startup parameter");
            }

            if (option.IsHelp)
            {
                var dialog = new MessageDialog(optionMap.GetCommandLineHelpText(), NeeView.Properties.Resources.DialogBootOptionTitle);
                dialog.SizeToContent = SizeToContent.WidthAndHeight;
                dialog.ShowDialog();
                throw new OperationCanceledException("Disp CommandLine Help");
            }

            return option;
        }
    }
}
