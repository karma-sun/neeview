using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ToggleStretchModeCommand : CommandElement
    {
        public ToggleStretchModeCommand(string name) : base(name)
        {
            this.Group = Properties.Resources.CommandGroupImageScale;
            this.Text = Properties.Resources.CommandToggleStretchMode;
            this.Note = Properties.Resources.CommandToggleStretchModeNote;
            this.ShortCutKey = "LeftButton+WheelDown";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter() { IsLoop = true });
        }

        public override string ExecuteMessage(CommandParameter param, object[] args, CommandOption option)
        {
            return ContentCanvas.Current.GetToggleStretchMode((ToggleStretchModeCommandParameter)param).ToAliasName();
        }

        public override bool CanExecute(CommandParameter param, object[] args, CommandOption option)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(CommandParameter param, object[] args, CommandOption option)
        {
            Config.Current.View.StretchMode = ContentCanvas.Current.GetToggleStretchMode((ToggleStretchModeCommandParameter)param);
        }
    }


    /// <summary>
    /// スケールモードトグル用設定
    /// </summary>
    [DataContract]
    public class ToggleStretchModeCommandParameter : CommandParameter
    {
        private Dictionary<PageStretchMode, bool> _strechModes;
        private bool _isLoop;


        // ループ
        [DataMember]
        [PropertyMember("@ParamCommandParameterToggleStretchLoop", Title = "@ParamCommandParameterToggleStretchLoopTitle")]
        public bool IsLoop
        {
            get => _isLoop;
            set => SetProperty(ref _isLoop, value);
        }

        // 表示名
        [DataMember]
        [PropertyMember("@EnumPageStretchModeNone", Title = "@ParamCommandParameterToggleStretchAllowTitle")]
        public bool IsEnableNone
        {
            get { return StretchModes[PageStretchMode.None]; }
            set { if (StretchModes[PageStretchMode.None] != value) { StretchModes[PageStretchMode.None] = value; RaisePropertyChanged(); } }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniform")]
        public bool IsEnableUniform
        {
            get { return StretchModes[PageStretchMode.Uniform]; }
            set { if (StretchModes[PageStretchMode.Uniform] != value) { StretchModes[PageStretchMode.Uniform] = value; RaisePropertyChanged(); } }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToFill")]
        public bool IsEnableUniformToFill
        {
            get { return StretchModes[PageStretchMode.UniformToFill]; }
            set { if (StretchModes[PageStretchMode.UniformToFill] != value) { StretchModes[PageStretchMode.UniformToFill] = value; RaisePropertyChanged(); } }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToSize")]
        public bool IsEnableUniformToSize
        {
            get { return StretchModes[PageStretchMode.UniformToSize]; }
            set { if (StretchModes[PageStretchMode.UniformToSize] != value) { StretchModes[PageStretchMode.UniformToSize] = value; RaisePropertyChanged(); } }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToVertical")]
        public bool IsEnableUniformToVertical
        {
            get { return StretchModes[PageStretchMode.UniformToVertical]; }
            set { if (StretchModes[PageStretchMode.UniformToVertical] != value) { StretchModes[PageStretchMode.UniformToVertical] = value; RaisePropertyChanged(); } }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToHorizontal")]
        public bool IsEnableUniformToHorizontal
        {
            get { return StretchModes[PageStretchMode.UniformToHorizontal]; }
            set { if (StretchModes[PageStretchMode.UniformToHorizontal] != value) { StretchModes[PageStretchMode.UniformToHorizontal] = value; RaisePropertyChanged(); } }
        }


        [JsonIgnore, EqualsIgnore]
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
    }


}
