using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace NeeView
{
    public class ScriptManager
    {
        private CommandTable _commandTable;
        private bool _isDarty = true;
        private ScriptUnitPool _pool = new ScriptUnitPool();


        public ScriptManager(CommandTable commandTable)
        {
            _commandTable = commandTable;
            Config.Current.Script.AddPropertyChanged(nameof(ScriptConfig.IsScriptFolderEnabled), ScriptConfigChanged);
            Config.Current.Script.AddPropertyChanged(nameof(ScriptConfig.ScriptFolder), ScriptConfigChanged);
        }


        private void ScriptConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateScriptCommands(isForce: true, isReplace: false);
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
                }
                ExternalProcess.Start("explorer.exe", $"\"{path}\"", ExternalProcessAtrtibute.ThrowException);
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.Resources.OpenScriptsFolderErrorDialog_Title).ShowDialog();
            }
        }



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

        public void Execute(object sender, string path)
        {
            _pool.Run(sender, path);
        }

        public void CancelAll()
        {
            _pool.CancelAll();
        }
    }
}
