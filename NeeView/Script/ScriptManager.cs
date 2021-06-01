using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace NeeView
{
    public class ScriptManager : IDisposable
    {
        private CommandTable _commandTable;
        private bool _isDarty = true;
        private ScriptUnitPool _pool = new ScriptUnitPool();
        private ScriptFolderWatcher _watcher;
        private bool _disposedValue;

        public ScriptManager(CommandTable commandTable)
        {
            _commandTable = commandTable;

            _watcher = new ScriptFolderWatcher();
            _watcher.Changed += (s, e) => UpdateScriptCommands(true, false);

            Config.Current.Script.AddPropertyChanged(nameof(ScriptConfig.IsScriptFolderEnabled),
                (s, e) => ScriptConfigChanged());

            Config.Current.Script.AddPropertyChanged(nameof(ScriptConfig.ScriptFolder),
                (s, e) => ScriptConfigChanged());

            UpdateWatcher();
        }


        private void ScriptConfigChanged()
        {
            UpdateScriptCommands(isForce: true, isReplace: false);
            UpdateWatcher();
        }

        private void UpdateWatcher()
        {
            if (Config.Current.Script.IsScriptFolderEnabled)
            {
                _watcher.Start(Config.Current.Script.ScriptFolder);
            }
            else
            {
                _watcher.Stop();
            }
        }

        public void OpenScriptsFolder()
        {
            var path = Config.Current.Script.ScriptFolder;
            if (string.IsNullOrEmpty(path))
            {
                new MessageDialog(Properties.Resources.OpenScriptsFolderErrorDialog_FolderIsNotSet, Properties.Resources.OpenScriptsFolderErrorDialog_Title).ShowDialog();
                return;
            }

            try
            {
                var directory = new DirectoryInfo(path);
                if (!directory.Exists)
                {
                    directory.Create();
                    ResourceTools.ExportFileFromResource(System.IO.Path.Combine(directory.FullName, "Sample.nvjs"), "/Resources/Scripts/Sample.nvjs");
                    ScriptConfigChanged();
                }
                ExternalProcess.Start("explorer.exe", $"\"{path}\"", new ExternalProcessOptions() { IsThrowException = true });
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.OpenScriptsFolderErrorDialog_Title).ShowDialog();
            }
        }


        /// <summary>
        /// コマンドテーブルのスクリプトコマンド更新要求
        /// </summary>
        /// <param name="isForce">強制実行</param>
        /// <param name="isReplace">登録済スクリプトも置き換える</param>
        /// <returns>実行した</returns>
        public bool UpdateScriptCommands(bool isForce, bool isReplace)
        {
            if (!isForce && !_isDarty) return false;
            _isDarty = false;

            List<ScriptCommand> commands = null;

            if (Config.Current.Script.IsScriptFolderEnabled)
            {
                commands = CollectScripts()
                    .Select(e => new ScriptCommand(ScriptCommand.Prefix + Path.GetFileNameWithoutExtension(e.Name)))
                    .ToList();

                foreach (var command in commands)
                {
                    try
                    {
                        command.LoadDocComment();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    command.StoreDefault();
                }
            }

            _commandTable.SetScriptCommands(commands, isReplace);
            return true;
        }

        public static List<FileInfo> CollectScripts()
        {
            if (!string.IsNullOrEmpty(Config.Current.Script.ScriptFolder))
            {
                try
                {
                    var directory = new DirectoryInfo(Config.Current.Script.ScriptFolder);
                    if (directory.Exists)
                    {
                        return directory.GetFiles("*" + ScriptCommand.Extension).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return new List<FileInfo>();
        }

        public void Execute(object sender, string path, string argument)
        {
            _pool.Run(sender, path, argument);
        }

        public void CancelAll()
        {
            _pool.CancelAll();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    CancelAll();
                    _watcher.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
