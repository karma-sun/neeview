// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        //
        public static new App Current => (App)Application.Current;
        public new MainWindow MainWindow => (MainWindow)base.MainWindow;

        // オプション設定
        public CommandLineOption Option { get; private set; }


        /// <summary>
        /// Startup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // 環境初期化
            Config.Current.Initiallize();

            // カレントフォルダー設定
            System.Environment.CurrentDirectory = Config.Current.LocalApplicationDataPath;

            // コマンドライン引数処理
            try
            {
                this.Option = ParseArguments(e.Args);
                this.Option.Validate();
            }
            catch (CommandLineHelpException)
            {
                this.Shutdown(0);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "起動オプションエラー", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown(1);
                return;
            }


            // 設定ファイルの読み込み
            SaveData.Current.LoadSetting(Option.SettingFilename);
            var setting = SaveData.Current.Setting;

            // restore
            Restore(setting.App);
            RestoreCompatible(setting);


            // 多重起動チェック
            Process currentProcess = Process.GetCurrentProcess();

            bool isNewWindow = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
                || Option.IsNewWindow != null ? Option.IsNewWindow == SwitchOption.on : IsMultiBootEnabled;

            if (!isNewWindow)
            {
                // 自身と異なるプロセスを見つけ、サーバとする
                var processName = currentProcess.ProcessName;
#if DEBUG
                var processes = Process.GetProcessesByName(processName)
                    .Concat(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentProcess.ProcessName)))
                    .ToList();
#else
                var processes = Process.GetProcessesByName(processName)
                    .ToList();
#endif

                // 最も古いプロセスを残す
                var serverProcess = processes
                    .OrderByDescending((p) => p.StartTime)
                    .FirstOrDefault((p) => p.Id != currentProcess.Id);

                if (serverProcess != null)
                {
                    try
                    {
                        Win32Api.AllowSetForegroundWindow(serverProcess.Id);
                        // IPCクライアント送信
                        IpcRemote.LoadAs(serverProcess.Id, Option.StartupPlace);

                        // 起動を中止してプログラムを終了
                        this.Shutdown();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("起動を継続します。\n 理由：" + ex.Message);
                    }
                }
            }

            // IPCサーバ起動
            IpcRemote.BootServer(currentProcess.Id);

            // メインウィンドウ起動
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }



        /// <summary>
        /// 終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // IPC シャットダウン
            Debug.WriteLine("IpcServer_Shutdown");
            IpcRemote.Shutdown();

            // プロセスを確実に終了させるための保険
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(5000);
                Debug.WriteLine("Environment_Exit");
                System.Environment.Exit(0);
            });

            Debug.WriteLine("Application_Exit");
        }

    }
}
