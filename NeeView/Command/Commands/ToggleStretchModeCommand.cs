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

            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter());
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
        private bool _isLoop = true;
        private bool _isEnableNone = true;
        private bool _isEnableUniform = true;
        private bool _isEnableUniformToFill = true;
        private bool _isEnableUniformToSize = true;
        private bool _isEnableUniformToVertical = true;
        private bool _isEnableUniformToHorizontal = true;

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
            get => _isEnableNone;
            set => SetProperty(ref _isEnableNone, value);
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniform")]
        public bool IsEnableUniform
        {
            get => _isEnableUniform;
            set => SetProperty(ref _isEnableUniform, value);
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToFill")]
        public bool IsEnableUniformToFill
        {
            get => _isEnableUniformToFill;
            set => SetProperty(ref _isEnableUniformToFill, value);
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToSize")]
        public bool IsEnableUniformToSize
        {
            get => _isEnableUniformToSize;
            set => SetProperty(ref _isEnableUniformToSize, value);
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToVertical")]
        public bool IsEnableUniformToVertical
        {
            get => _isEnableUniformToVertical;
            set => SetProperty(ref _isEnableUniformToVertical, value);
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeUniformToHorizontal")]
        public bool IsEnableUniformToHorizontal
        {
            get => _isEnableUniformToHorizontal;
            set => SetProperty(ref _isEnableUniformToHorizontal, value);
        }


        public IReadOnlyDictionary<PageStretchMode, bool> GetStretchModeDictionary()
        {
            return new Dictionary<PageStretchMode, bool>()
            {
                [PageStretchMode.None] = IsEnableNone,
                [PageStretchMode.Uniform] = IsEnableUniform,
                [PageStretchMode.UniformToFill] = IsEnableUniformToFill,
                [PageStretchMode.UniformToSize] = IsEnableUniformToSize,
                [PageStretchMode.UniformToVertical] = IsEnableUniformToVertical,
                [PageStretchMode.UniformToHorizontal] = IsEnableUniformToHorizontal,
            };
        }
    }

}
