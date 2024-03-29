﻿using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ViewScrollUpCommand : CommandElement
    {
        public ViewScrollUpCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter());
        }
        public override void Execute(object sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ScrollUp((ViewScrollCommandParameter)e.Parameter);
        }
    }


    /// <summary>
    /// ビュースクロールコマンド用パラメータ
    /// </summary>
    [DataContract]
    public class ViewScrollCommandParameter : CommandParameter
    {
        private double _scroll = 0.25;
        private bool _allowCrossScroll = true;
        private double _scrollDuration = 0.1;

        // 属性に説明文
        [DataMember(Name = "ScrollV2")]
        [PropertyPercent]
        public double Scroll
        {
            get { return _scroll; }
            set { SetProperty(ref _scroll, MathUtility.Clamp(value, 0.0, 1.0)); }
        }

        // スクロール速度(秒)
        [DataMember]
        [PropertyMember]
        public double ScrollDuration
        {
            get { return _scrollDuration; }
            set { SetProperty(ref _scrollDuration, Math.Max(value, 0.0)); }
        }

        [DataMember]
        [PropertyMember]
        public bool AllowCrossScroll
        {
            get => _allowCrossScroll;
            set => SetProperty(ref _allowCrossScroll, value);
        }


        #region Obsolete
        [Obsolete, JsonIgnore, EqualsIgnore, DataMember(Name = "Scroll", EmitDefaultValue = false)] // ver.37
        [PropertyMapIgnore]
        public int ScrollV1
        {
            get => 0;
            set => Scroll = value / 100.0;
        }
        #endregion


        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.Scroll = 0.25;
            this.AllowCrossScroll = true;
            this.ScrollDuration = 0.1;
        }

    }

}
