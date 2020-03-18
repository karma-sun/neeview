using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.IO;

namespace NeeView
{
    public class ScriptConfig : BindableBase
    {
        private bool _isScriptFolderEnabled;
        private string _scriptFolder = string.Empty;


        public string DefaultScriptFolderName => "Scripts";

        [PropertyMember("@ParamIsScriptFolderEnabled")]
        public bool IsScriptFolderEnabled
        {
            get { return _isScriptFolderEnabled; }
            set { SetProperty(ref _isScriptFolderEnabled, value); }
        }

        [PropertyPath("@ParamScriptFolder", Tips = "@ParamScriptFolderTips", FileDialogType = Windows.Controls.FileDialogType.Directory)]
        public string ScriptFolder
        {
            get { return _scriptFolder; }
            set
            {
                var path = value?.Trim();
                if (string.IsNullOrEmpty(path) || path == GetDefaultScriptFolder())
                {
                    path = string.Empty;
                }
                SetProperty(ref _scriptFolder, path);
            }
        }


        public string GetCurrentScriptFolder()
        {
            return string.IsNullOrEmpty(ScriptFolder) ? GetDefaultScriptFolder() : ScriptFolder;
        }

        public string GetDefaultScriptFolder()
        {
            if (Environment.IsZipLikePackage)
            {
                return Path.Combine(Environment.LocalApplicationDataPath, DefaultScriptFolderName);
            }
            else
            {
                return Path.Combine(Environment.GetMyDocumentPath(false), DefaultScriptFolderName);
            }
        }
    }
}