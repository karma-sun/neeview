using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NeeLaboratory;
using NeeView.Data;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// コマンドパラメータ（基底）
    /// </summary>
    [DataContract]
    [JsonConverter(typeof(JsonCommandParameterConverter))]
    public abstract class CommandParameter : ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }

        public abstract bool MemberwiseEquals(CommandParameter other);


        #region Legacy

        // 共有制御用 (Obsolete)
        public virtual bool IsReadOnly()
        {
            return false;
        }

        /// <summary>
        /// 実際に適用されるパラメータ (Obsolete)
        /// </summary>
        public virtual CommandParameter Entity()
        {
            return this;
        }

        public object ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            var type = this.GetType();

            foreach (PropertyInfo info in type.GetProperties())
            {
                var attribute = (PropertyMemberAttribute)Attribute.GetCustomAttributes(info, typeof(PropertyMemberAttribute)).FirstOrDefault();
                if (attribute != null)
                {
                    dictionary.Add(info.Name, info.GetValue(this));
                }
            }

            return dictionary;
        }

        #endregion
    }


    public sealed class JsonCommandParameterConverter : JsonConverter<CommandParameter>
    {
        public static Type[] KnownTypes { get; set; } = new Type[]
        {
            typeof(ReversibleCommandParameter),
            typeof(MoveSizePageCommandParameter),
            typeof(ToggleStretchModeCommandParameter),
            typeof(StretchModeCommandParameter),
            typeof(ViewScrollCommandParameter),
            typeof(ViewScaleCommandParameter),
            typeof(ViewRotateCommandParameter),
            typeof(MovePagemarkCommandParameter),
            typeof(ScrollPageCommandParameter),
            typeof(FocusMainViewCommandParameter),
            typeof(ExportImageDialogCommandParameter),
            typeof(ExportImageCommandParameter),
            typeof(OpenExternalAppCommandParameter),
            typeof(CopyFileCommandParameter),
        };

        public static JsonSerializerOptions GetSerializerOptions()
        {
            var options = new JsonSerializerOptions();
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.WriteIndented = false;
            options.IgnoreReadOnlyProperties = false;
            options.IgnoreNullValues = false;
            return options;
        }

        public override CommandParameter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "Type")
            {
                throw new JsonException();
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }
            var typeString = reader.GetString();

            Type type = KnownTypes.FirstOrDefault(e => e.Name == typeString);
            Debug.Assert(type != null);

            if (!reader.Read() || reader.GetString() != "Value")
            {
                throw new JsonException();
            }
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            object instance;
            if (type != null)
            {
                instance = JsonSerializer.Deserialize(ref reader, type, options);
            }
            else
            {
                Debug.WriteLine($"Nor support type: {typeString}");
                reader.Skip();
                instance = null;
            }

            if (!reader.Read() || reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return (CommandParameter)instance;
        }

        public override void Write(Utf8JsonWriter writer, CommandParameter value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            var type = value.GetType();
            writer.WriteString("Type", type.Name);
            writer.WritePropertyName("Value");
            JsonSerializer.Serialize(writer, value, type, options);

            writer.WriteEndObject();
        }


    }





    // 操作反転パラメータ基底
    [DataContract]
    public class ReversibleCommandParameter : CommandParameter
    {
        [DataMember]
        [PropertyMember("@ParamCommandParameterIsReverse", Tips = "@ParamCommandParameterIsReverseTips")]
        public bool IsReverse { get; set; } = true;

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            IsReverse = true;
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ReversibleCommandParameter;
            if (target == null) return false;
            return this == target || (this.IsReverse == target.IsReverse);
        }
    }


    /// <summary>
    /// 指定ページ数移動コマンド用パラメータ
    /// </summary>
    public class MoveSizePageCommandParameter : ReversibleCommandParameter
    {
        [PropertyMember("@ParamCommandParameterMoveSize")]
        public int Size
        {
            get { return _size; }
            set { _size = MathUtility.Clamp(value, 0, 1000); }
        }
        private int _size;

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as MoveSizePageCommandParameter;
            if (target == null) return false;
            return this == target || (this.Size == target.Size);
        }
    }

    /// <summary>
    /// スケールモードトグル用設定
    /// </summary>
    [DataContract]
    public class ToggleStretchModeCommandParameter : CommandParameter
    {
        // ループ
        [DataMember]
        [PropertyMember("@ParamCommandParameterToggleStretchLoop", Title = "@ParamCommandParameterToggleStretchLoopTitle")]
        public bool IsLoop { get; set; }

        // 表示名
        [DataMember]
        [PropertyMember("@EnumPageStretchModeNone", Title = "@ParamCommandParameterToggleStretchAllowTitle")]
        public bool IsEnableNone
        {
            get { return StretchModes[PageStretchMode.None]; }
            set { StretchModes[PageStretchMode.None] = value; }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniform")]
        public bool IsEnableUniform
        {
            get { return StretchModes[PageStretchMode.Uniform]; }
            set { StretchModes[PageStretchMode.Uniform] = value; }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToFill")]
        public bool IsEnableUniformToFill
        {
            get { return StretchModes[PageStretchMode.UniformToFill]; }
            set { StretchModes[PageStretchMode.UniformToFill] = value; }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToSize")]
        public bool IsEnableUniformToSize
        {
            get { return StretchModes[PageStretchMode.UniformToSize]; }
            set { StretchModes[PageStretchMode.UniformToSize] = value; }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToVertical")]
        public bool IsEnableUniformToVertical
        {
            get { return StretchModes[PageStretchMode.UniformToVertical]; }
            set { StretchModes[PageStretchMode.UniformToVertical] = value; }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToHorizontal")]
        public bool IsEnableUniformToHorizontal
        {
            get { return StretchModes[PageStretchMode.UniformToHorizontal]; }
            set { StretchModes[PageStretchMode.UniformToHorizontal] = value; }
        }

        private Dictionary<PageStretchMode, bool> _strechModes;

        [JsonIgnore]
        public Dictionary<PageStretchMode, bool> StretchModes
        {
            get
            {
                if (_strechModes == null)
                {
                    _strechModes = Enum.GetValues(typeof(PageStretchMode)).Cast<PageStretchMode>().ToDictionary(e => e, e => true);
                }
                return _strechModes;
            }
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ToggleStretchModeCommandParameter;
            if (target == null) return false;
            return this == target || (this.IsLoop == target.IsLoop &&
                this.IsEnableNone == target.IsEnableNone &&
                this.IsEnableUniform == target.IsEnableUniform &&
                this.IsEnableUniformToFill == target.IsEnableUniformToFill &&
                this.IsEnableUniformToSize == target.IsEnableUniformToSize &&
                this.IsEnableUniformToVertical == target.IsEnableUniformToVertical &&
                this.IsEnableUniformToHorizontal == target.IsEnableUniformToHorizontal);
        }
    }


    /// <summary>
    /// スケールモード用設定
    /// </summary>
    public class StretchModeCommandParameter : CommandParameter
    {
        // 属性に説明文
        [PropertyMember("@ParamCommandParameterStretchModeIsToggle", Tips = "@ParamCommandParameterStretchModeIsToggleTips")]
        public bool IsToggle { get; set; }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as StretchModeCommandParameter;
            if (target == null) return false;
            return this == target || (this.IsToggle == target.IsToggle);
        }
    }

    /// <summary>
    /// ビュースクロールコマンド用パラメータ
    /// </summary>
    [DataContract]
    public class ViewScrollCommandParameter : CommandParameter
    {
        // 属性に説明文
        [DataMember]
        [PropertyRange("@ParamCommandParameterScrollAmount", 0, 100, Tips = "@ParamCommandParameterScrollAmountTips")]
        public int Scroll
        {
            get { return _scroll; }
            set { _scroll = MathUtility.Clamp(value, 0, 100); }
        }
        private int _scroll;

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollAllowCross", Tips = "@ParamCommandParameterScrollAllowCrossTips")]
        public bool AllowCrossScroll { get; set; } = true;

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.AllowCrossScroll = true;
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ViewScrollCommandParameter;
            if (target == null) return false;
            return this == target || (this.Scroll == target.Scroll && this.AllowCrossScroll == target.AllowCrossScroll);
        }
    }


    /// <summary>
    /// ビュー拡大コマンド用パラメータ
    /// </summary>
    public class ViewScaleCommandParameter : CommandParameter
    {
        // 属性に説明文
        [PropertyRange("@ParamCommandParameterScaleAmount", 0, 100, Tips = "@ParamCommandParameterScaleAmountTips")]
        public int Scale
        {
            get { return _scale; }
            set { _scale = MathUtility.Clamp(value, 0, 100); }
        }
        private int _scale;

        [DataMember, DefaultValue(true)]
        [PropertyMember("@ParamCommandParameterScaleSnapDefault", Tips = "@ParamCommandParameterScaleSnapDefaultTips")]
        public bool IsSnapDefaultScale { get; set; } = true;

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.IsSnapDefaultScale = true;
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ViewScaleCommandParameter;
            if (target == null) return false;
            return this == target || (this.Scale == target.Scale && this.IsSnapDefaultScale == target.IsSnapDefaultScale);
        }
    }

    /// <summary>
    /// ビュー回転コマンド用パラメータ
    /// </summary>
    public class ViewRotateCommandParameter : CommandParameter
    {
        // 属性に説明文
        [PropertyRange("@ParamCommandParameterRotateAmount", 0, 180, Tips = "@ParamCommandParameterRotateAmountTips")]
        public int Angle
        {
            get { return _angle; }
            set { _angle = MathUtility.Clamp(value, 0, 180); }
        }
        private int _angle;

        // 属性に説明文
        [PropertyMember("@ParamCommandParameterRotateIsStretch", Tips = "@ParamCommandParameterRotateIsStretchTips")]
        public bool IsStretch { get; set; }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ViewRotateCommandParameter;
            if (target == null) return false;
            return this == target || (this.Angle == target.Angle && this.IsStretch == target.IsStretch);
        }
    }


    /// <summary>
    /// ページマーク移動用パラメータ
    /// </summary>
    public class MovePagemarkCommandParameter : CommandParameter
    {
        [PropertyMember("@ParamCommandParameterMovePagemarkLoop")]
        public bool IsLoop { get; set; }

        [PropertyMember("@ParamCommandParameterMovePagemarkIncludeTerminal")]
        public bool IsIncludeTerminal { get; set; }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as MovePagemarkCommandParameter;
            if (target == null) return false;
            return this == target || (this.IsLoop == target.IsLoop && this.IsIncludeTerminal == target.IsIncludeTerminal);
        }
    }

    /// <summary>
    /// スクロール＋ページ移動用パラメータ
    /// </summary>
    [DataContract]
    public class ScrollPageCommandParameter : ReversibleCommandParameter
    {
        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageN", Tips = "@ParamCommandParameterScrollPageNTips")]
        public bool IsNScroll { get; set; }

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageAnimation")]
        public bool IsAnimation { get; set; }

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageMargin", Tips = "@ParamCommandParameterScrollPageMarginTips")]
        public double Margin { get; set; }

        [DataMember]
        [PropertyRange("@ParamCommandParameterScrollPageAmount", 0, 100, Tips = "@ParamCommandParameterScrollPageAmountTips")]
        public int Scroll
        {
            get { return _scroll; }
            set { _scroll = MathUtility.Clamp(value, 0, 100); }
        }
        private int _scroll;

        [DataMember]
        [PropertyMember("@ParamCommandParameterScrollPageStop", Tips = "@ParamCommandParameterScrollPageStopTips")]
        public bool IsStop { get; set; }


        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _scroll = 100;
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ScrollPageCommandParameter;
            if (target == null) return false;
            return this == target || (this.IsNScroll == target.IsNScroll &&
                this.IsAnimation == target.IsAnimation &&
                this.Margin == target.Margin &&
                this.Scroll == target.Scroll &&
                this.IsStop == target.IsStop);
        }
    }


    [DataContract]
    public class FocusMainViewCommandParameter : CommandParameter
    {
        [DataMember]
        [PropertyMember("@ParamCommandParameterFocusMainViewClosePanels")]
        public bool NeedClosePanels { get; set; }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as FocusMainViewCommandParameter;
            if (target == null) return false;
            return this == target || (this.NeedClosePanels == target.NeedClosePanels);
        }
    }


    [DataContract]
    public class ExportImageDialogCommandParameter : CommandParameter
    {
        [DataMember, PropertyPath("@ParamCommandParameterExportDefaultFolder", Tips = "@ParamCommandParameterExportDefaultFolderTips", FileDialogType = FileDialogType.Directory)]
        public string ExportFolder { get; set; }

        [DataMember, PropertyRange("@ParamCommandParameterExportImageQualityLevel", 5, 100, TickFrequency = 5, Tips = "@ParamCommandParameterExportImageQualityLevelTips")]
        public int QualityLevel { get; set; } = 80;

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ExportImageDialogCommandParameter;
            if (target == null) return false;
            return this == target || (this.ExportFolder == target.ExportFolder && this.QualityLevel == target.QualityLevel);
        }
    }


    [DataContract]
    public class ExportImageCommandParameter : CommandParameter
    {
        [DataMember, PropertyMember("@ParamCommandParameterExportMode")]
        public ExportImageMode Mode { get; set; }

        [DataMember, PropertyMember("@ParamCommandParameterExportHasBackground")]
        public bool HasBackground { get; set; }

        [DataMember, PropertyPath("@ParamCommandParameterExportFolder", FileDialogType = FileDialogType.Directory)]
        public string ExportFolder { get; set; }

        [DataMember, PropertyMember("@ParamCommandParameterExportFileNameMode")]
        public ExportImageFileNameMode FileNameMode { get; set; }

        [DataMember, PropertyMember("@ParamCommandParameterExportFileFormat", Tips = "@ParamCommandParameterExportFileFormat")]
        public ExportImageFormat FileFormat { get; set; }

        [DataMember, PropertyRange("@ParamCommandParameterExportImageQualityLevel", 5, 100, TickFrequency = 5, Tips = "@ParamCommandParameterExportImageQualityLevelTips")]
        public int QualityLevel { get; set; } = 80;

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ExportImageCommandParameter;
            if (target == null) return false;
            return this == target || (this.Mode == target.Mode &&
                this.HasBackground == target.HasBackground &&
                this.ExportFolder == target.ExportFolder &&
                this.FileNameMode == target.FileNameMode &&
                this.FileFormat == target.FileFormat &&
                this.QualityLevel == target.QualityLevel);
        }
    }


    public class OpenExternalAppCommandParameter : CommandParameter, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        // コマンドパラメータで使用されるキーワード
        public const string KeyFile = "$File";
        public const string KeyUri = "$Uri";
        public const string DefaultParameter = "\"" + KeyFile + "\"";

        private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        private string _archiveSeparater = "\\";
        private string _command;
        private string _parameter = DefaultParameter;
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;


        // コマンド
        [DataMember]
        [PropertyPath("@ParamExternalCommand", Tips = "@ParamExternalCommandTips", Filter = "EXE|*.exe|All|*.*")]
        public string Command
        {
            get { return _command; }
            set { SetProperty(ref _command, value); }
        }

        // コマンドパラメータ
        // $FILE = 渡されるファイルパス
        [DataMember]
        [PropertyMember("@ParamExternalParameter", Tips = "@ParamExternalParameterTips")]
        public string Parameter
        {
            get { return _parameter; }
            set { SetProperty(ref _parameter, string.IsNullOrWhiteSpace(value) ? DefaultParameter : value); }
        }

        // 複数ページのときの動作
        [DataMember]
        [PropertyMember("@ParamExternalMultiPageOption")]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { SetProperty(ref _multiPagePolicy, value); }
        }

        // 圧縮ファイルのときの動作
        [DataMember]
        [PropertyMember("@ParamExternalArchiveOption")]
        public ArchivePolicy ArchivePolicy
        {
            get { return _archivePolicy; }
            set { SetProperty(ref _archivePolicy, value); }
        }

        [DataMember(EmitDefaultValue = false)]
        [PropertyMember("@ParamExternalArchiveSeparater", Tips = "@ParamExternalArchiveSeparaterTips", EmptyMessage = "\\")]
        public string ArchiveSeparater
        {
            get { return _archiveSeparater; }
            set { SetProperty(ref _archiveSeparater, string.IsNullOrEmpty(value) ? "\\" : value); }
        }

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as OpenExternalAppCommandParameter;
            if (target == null) return false;
            return this == target || (this.Command == target.Command &&
                this.Parameter == target.Parameter &&
                this.MultiPagePolicy == target.MultiPagePolicy &&
                this.ArchivePolicy == target.ArchivePolicy &&
                this.ArchiveSeparater == target.ArchiveSeparater);
        }
    }


    public class CopyFileCommandParameter : CommandParameter, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        private string _archiveSeparater = "\\";
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;


        // 複数ページのときの動作
        [DataMember]
        [PropertyMember("@ParamClipboardMultiPageOption")]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { SetProperty(ref _multiPagePolicy, value); }
        }

        // 圧縮ファイルのときの動作
        [DataMember]
        [PropertyMember("@ParamClipboardArchiveOption")]
        public ArchivePolicy ArchivePolicy
        {
            get { return _archivePolicy; }
            set { SetProperty(ref _archivePolicy, value); }
        }

        [DataMember(EmitDefaultValue = false)]
        [PropertyMember("@ParamClipboardArchiveSeparater", Tips = "@ParamClipboardArchiveSeparaterTips", EmptyMessage = "\\")]
        public string ArchiveSeparater
        {
            get { return _archiveSeparater; }
            set { SetProperty(ref _archiveSeparater, string.IsNullOrEmpty(value) ? "\\" : value); }
        }


        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as CopyFileCommandParameter;
            if (target == null) return false;
            return this == target || (
                this.MultiPagePolicy == target.MultiPagePolicy &&
                this.ArchivePolicy == target.ArchivePolicy &&
                this.ArchiveSeparater == target.ArchiveSeparater);
        }
    }
}
