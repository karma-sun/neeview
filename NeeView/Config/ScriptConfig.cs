using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.IO;

namespace NeeView
{
    public class ScriptConfig : BindableBase
    {
        private bool _isScriptFolderEnabled;
        private string _scriptFolder = string.Empty;


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
            set { SetProperty(ref _scriptFolder, (string.IsNullOrEmpty(value) || value?.Trim() == GetDefaultScriptFolder()) ? string.Empty : value); }
        }


        public string GetDefaultScriptFolderName() => "Scripts";

        public string GetCurrentScriptFolder()
        {
            return string.IsNullOrEmpty(ScriptFolder) ? GetDefaultScriptFolder() : ScriptFolder;
        }

        public string GetDefaultScriptFolder()
        {
            if (Environment.IsZipLikePackage)
            {
                return Path.Combine(Environment.LocalApplicationDataPath, GetDefaultScriptFolderName());
            }
            else
            {
                return Path.Combine(Environment.GetMyDocumentPath(false), GetDefaultScriptFolderName());
            }
        }
    }
}