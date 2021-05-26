using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System.IO;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ScriptConfig : BindableBase
    {
        private bool _isScriptFolderEnabled;
        private ScriptErrorLevel _errorLevel = ScriptErrorLevel.Error;


        [JsonInclude]
        [JsonPropertyName(nameof(ScriptFolder))]
        public string _scriptFolder = null;


        [PropertyMember]
        public bool IsScriptFolderEnabled
        {
            get { return _isScriptFolderEnabled; }
            set { SetProperty(ref _isScriptFolderEnabled, value); }
        }

        [JsonIgnore]
        [PropertyPath(FileDialogType = Windows.Controls.FileDialogType.Directory)]
        public string ScriptFolder
        {
            get { return _scriptFolder ?? SaveData.DefaultScriptsFolder; }
            set { SetProperty(ref _scriptFolder, (string.IsNullOrEmpty(value) || value.Trim() == SaveData.DefaultScriptsFolder) ? null : value.Trim()); }
        }

        [PropertyMember]
        public ScriptErrorLevel ErrorLevel
        {
            get { return _errorLevel; }
            set { SetProperty(ref _errorLevel, value); }
        }
    }
}