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

        public override string ExecuteMessage(object sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewController.GetToggleStretchMode((ToggleStretchModeCommandParameter)e.Parameter).ToAliasName();
        }

        public override bool CanExecute(object sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object sender, CommandContext e)
        {
            Config.Current.View.StretchMode = MainViewComponent.Current.ViewController.GetToggleStretchMode((ToggleStretchModeCommandParameter)e.Parameter);
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
        [PropertyMember("@PageStretchMode.None", Title = "@ParamCommandParameterToggleStretchAllowTitle")]
        public bool IsEnableNone
        {
            get => _isEnableNone;
            set => SetProperty(ref _isEnableNone, value);
        }

        [DataMember]
        [PropertyMember("@PageStretchMode.Uniform")]
        public bool IsEnableUniform
        {
            get => _isEnableUniform;
            set => SetProperty(ref _isEnableUniform, value);
        }

        [DataMember]
        [PropertyMember("@PageStretchMode.UniformToFill")]
        public bool IsEnableUniformToFill
        {
            get => _isEnableUniformToFill;
            set => SetProperty(ref _isEnableUniformToFill, value);
        }

        [DataMember]
        [PropertyMember("@PageStretchMode.UniformToSize")]
        public bool IsEnableUniformToSize
        {
            get => _isEnableUniformToSize;
            set => SetProperty(ref _isEnableUniformToSize, value);
        }

        [DataMember]
        [PropertyMember("@PageStretchMode.UniformToVertical")]
        public bool IsEnableUniformToVertical
        {
            get => _isEnableUniformToVertical;
            set => SetProperty(ref _isEnableUniformToVertical, value);
        }

        [DataMember]
        [PropertyMember("@PageStretchMode.UniformToHorizontal")]
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
