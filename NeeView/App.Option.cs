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
    //
    public enum SwitchOption
    {
        off,
        on,
    }

    //
    public class CommandLineOption
    {
        [OptionMember("h", "help", Default = "true", HelpText = "@OptionIsHelp")]
        public bool IsHelp { get; set; }

        [OptionMember("v", "version", Default = "true", HelpText = "@OptionIsVersion")]
        public bool IsVersion { get; set; }

        [OptionMember("x", "setting", HasParameter = true, RequireParameter = true, HelpText = "@OptionSettingFilename")]
        public string SettingFilename { get; set; }

        [OptionMember("f", "fullscreen", Default = "on", HasParameter = true, HelpText = "@OptionIsFullScreen")]
        public SwitchOption? IsFullScreen { get; set; }

        [OptionMember("b", "blank", Default = "on", HelpText = "@OptionIsBlank")]
        public SwitchOption IsBlank { get; set; }

        [OptionMember("r", "reset-placement", Default = "on", HelpText = "@OptionIsResetPlacement")]
        public SwitchOption IsResetPlacement { get; set; }

        [OptionMember("n", "new-window", Default = "on", HasParameter = true, HelpText = "@OptionIsNewWindow")]
        public SwitchOption? IsNewWindow { get; set; }

        [OptionMember("s", "slideshow", Default = "on", HasParameter = true, HelpText = "@OptionIsSlideShow")]
        public SwitchOption? IsSlideShow { get; set; }



        [OptionValues]
        public List<string> Values { get; set; }


        //
        public string StartupPlace { get; set; }

        //
        public void Validate()
        {
            try
            {
                // SettingFilename
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
                    this.SettingFilename = Path.Combine(Config.Current.LocalApplicationDataPath, SaveData.UserSettingFileName);
                }

                // StartupPlage
                this.StartupPlace = Values?.LastOrDefault();
                if (this.StartupPlace != null && (File.Exists(this.StartupPlace) || Directory.Exists(this.StartupPlace)))
                {
                    this.StartupPlace = Path.GetFullPath(this.StartupPlace);
                }
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.DialogBootErrorTitle).ShowDialog();
                throw;
            }

        }
    }


    public partial class App
    {
        //
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
                throw;
            }

            if (option.IsHelp)
            {
                new MessageDialog(GetCommandLineHelp(optionMap), NeeView.Properties.Resources.DialogBootOptionTitle).ShowDialog();
                throw new ApplicationException("Disp CommandLine Help");
            }

            return option;
        }

        //
        public string GetCommandLineHelp(OptionMap<CommandLineOption> optionMap)
        {
            return "Usage: NeeView.exe NeeView.exe [Options...] [File or Folder]\n\n"
                + optionMap.GetHelpText() + "\n"
                + "Example:\n"
                + "                NeeView.exe -f\n"
                + "                NeeView.exe -fs E:\\Pictures\n"
                + "                NeeView.exe --setting=\"C:\\Sample\\CustomUserSetting.xml\" --new-window=off";
        }

        //
        public string GetCommandLineHelp()
        {
            return GetCommandLineHelp(new OptionMap<CommandLineOption>());
        }

    }
}
