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
        [OptionMember("h", "help", Default = "true", HelpText = "@AppOption_IsHelp")]
        public bool IsHelp { get; set; }

        [OptionMember("v", "version", Default = "true", HelpText = "@AppOption_IsVersion")]
        public bool IsVersion { get; set; }

        [OptionMember("x", "setting", HasParameter = true, RequireParameter = true, HelpText = "@AppOption_SettingFilename")]
        public string SettingFilename { get; set; }

        [OptionMember("b", "blank", Default = "on", HelpText = "@AppOption_IsBlank")]
        public SwitchOption IsBlank { get; set; }

        [OptionMember("r", "reset-placement", Default = "on", HelpText = "@AppOption_IsResetPlacement")]
        public SwitchOption IsResetPlacement { get; set; }

        [OptionMember("n", "new-window", Default = "on", HasParameter = true, HelpText = "@AppOption_IsNewWindow")]
        public SwitchOption? IsNewWindow { get; set; }

        [OptionMember("s", "slideshow", Default = "on", HasParameter = true, HelpText = "@AppOption_IsSlideShow")]
        public SwitchOption? IsSlideShow { get; set; }

        [OptionMember("o", "folderlist", HasParameter = true, RequireParameter = true, HelpText = "@AppOption_FolderList")]
        public string FolderList { get; set; }

        [OptionMember(null, "window", HasParameter = true, RequireParameter = true, HelpText = "@AppOption_WindowState")]
        public WindowStateOption? WindowState { get; set; }

        [OptionMember(null, "script", HasParameter = true, RequireParameter = true, HelpText = "@AppOption_ScriptFile")]
        public string ScriptFile { get; set; }


        [OptionValues]
        public List<string> Values { get; set; }


        public void Validate()
        {
            try
            {
                Values = Values.Select(e => GetFullPath(e)).ToList();

                FolderList = GetFullQueryPath(FolderList)?.SimpleQuery;
                ScriptFile = GetFullPath(ScriptFile);

                if (this.SettingFilename != null)
                {
                    if (!File.Exists(this.SettingFilename))
                    {
                        throw new ArgumentException($"{Properties.Resources.OptionArgumentException_FileNotFound}: {this.SettingFilename}");
                    }
                    this.SettingFilename = Path.GetFullPath(this.SettingFilename);
                }
                else
                {
                    this.SettingFilename = Path.Combine(Environment.LocalApplicationDataPath, SaveData.UserSettingFileName);
                }
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.BootErrorDialog_Title).ShowDialog();
                throw new OperationCanceledException("Wrong startup parameter");
            }
        }


        private QueryPath GetFullQueryPath(string src)
        {
            if (src is null) return null;

            var query = new QueryPath(src);
            if (query.Scheme != QueryScheme.File)
            {
                return query;
            }

            return query.ReplacePath(GetFullPath(query.Path));
        }

        private string GetFullPath(string src)
        {
            if (src is null) return null;

            var path = src.Replace('/', '\\');

            if (Directory.Exists(path))
            {
                return System.IO.Path.GetFullPath(path);
            }

            try
            {
                return GetFullArchivePath(path);
            }
            catch (FileNotFoundException)
            {
                return src;
            }
        }

        private string GetFullArchivePath(string path)
        {
            if (path is null) return null;

            if (File.Exists(path))
            {
                return System.IO.Path.GetFullPath(path);
            }

            var index = path.LastIndexOf('\\');
            if (index < 0)
            {
                throw new FileNotFoundException();
            }

            var directory = path.Substring(0, index);
            var filename = path.Substring(index);

            directory = GetFullArchivePath(directory);

            return LoosePath.Combine(directory, filename);
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
                new MessageDialog(ex.Message, NeeView.Properties.Resources.BootErrorDialog_Title).ShowDialog();
                throw new OperationCanceledException("Wrong startup parameter");
            }

            if (option.IsHelp)
            {
                var dialog = new MessageDialog(optionMap.GetCommandLineHelpText(), NeeView.Properties.Resources.BootOptionDialog_Title);
                dialog.SizeToContent = SizeToContent.WidthAndHeight;
                dialog.ShowDialog();
                throw new OperationCanceledException("Disp CommandLine Help");
            }

            return option;
        }
    }
}
