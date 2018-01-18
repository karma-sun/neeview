// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        [OptionMember("h", "help", Default = "true", HelpText = "このヘルプを表示します")]
        public bool IsHelp { get; set; }

        [OptionMember("v", "version", Default = "true", HelpText = "バージョン情報を表示します")]
        public bool IsVersion { get; set; }

        [OptionMember("x", "setting", HasParameter = true, RequireParameter = true, HelpText = "設定ファイル(UserSetting.xml)のパスを指定します")]
        public string SettingFilename { get; set; }

        [OptionMember("f", "fullscreen", Default = "on", HasParameter = true, HelpText = "フルスクリーンで起動するかを指定します")]
        public SwitchOption? IsFullScreen { get; set; }

        [OptionMember("b", "blank", Default = "on", HelpText = "画像ファイルを開かずに起動します")]
        public SwitchOption IsBlank { get; set; }

        [OptionMember("r", "reset-placement", Default = "on", HelpText = "ウィンドウ座標を初期化します")]
        public SwitchOption IsResetPlacement { get; set; }

        [OptionMember("n", "new-window", Default = "on", HasParameter = true, HelpText = "新しいウィンドウで起動するかを指定します")]
        public SwitchOption? IsNewWindow { get; set; }

        [OptionMember("s", "slideshow", Default = "on", HasParameter = true, HelpText = "スライドショウを開始するかを指定します")]
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
                        throw new ArgumentException($"指定された設定ファイルが存在しません : {this.SettingFilename}");
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
                new MessageDialog(ex.Message, "NeeView 起動エラー").ShowDialog();
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
                new MessageDialog(ex.Message, "NeeView 起動エラー").ShowDialog();
                throw;
            }

            if (option.IsHelp)
            {
                new MessageDialog(GetCommandLineHelp(optionMap), "NeeView 起動オプション").ShowDialog();
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
