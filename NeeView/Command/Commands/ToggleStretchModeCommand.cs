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


}
