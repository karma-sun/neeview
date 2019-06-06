using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NeeLaboratory;
using NeeView.Data;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// コマンドパラメータ（基底）
    /// </summary>
    [DataContract]
    public class CommandParameter
    {
        public CommandParameter Clone()
        {
            return (CommandParameter)Json.Clone(this, this.GetType());
        }

        public string ToJson()
        {
            return Json.Serialize(this, this.GetType());
        }

        public virtual bool IsReadOnly()
        {
            return false;
        }

        /// <summary>
        /// 実際に適用されるパラメータ
        /// </summary>
        public virtual CommandParameter Entity()
        {
            return this;
        }
    }

    /// <summary>
    /// コマンド間パラメータ共有用特殊パラメータ
    /// </summary>
    public class ShareCommandParameter : CommandParameter
    {
        [PropertyMember("@ParamCommandParameterShare")]
        public CommandType CommandType { get; set; }

        //
        public override bool IsReadOnly()
        {
            return true;
        }

        /// <summary>
        /// 実際に適用されるパラメータ
        /// </summary>
        public override CommandParameter Entity()
        {
            return CommandTable.Current?[CommandType].Parameter;
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
        [PropertyMember("@EnumPageStretchModeInside")]
        public bool IsEnableInside
        {
            get { return StretchModes[PageStretchMode.Inside]; }
            set { StretchModes[PageStretchMode.Inside] = value; }
        }

        [DataMember]
        [PropertyMember("@EnumPageStretchModeOutside")]
        public bool IsEnableOutside
        {
            get { return StretchModes[PageStretchMode.Outside]; }
            set { StretchModes[PageStretchMode.Outside] = value; }
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

        //
        private Dictionary<PageStretchMode, bool> _strechModes;
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


    /// <summary>
    /// スケールモード用設定
    /// </summary>
    public class StretchModeCommandParameter : CommandParameter
    {
        // 属性に説明文
        [PropertyMember("@ParamCommandParameterStretchModeIsToggle", Tips = "@ParamCommandParameterStretchModeIsToggleTips")]
        public bool IsToggle { get; set; }
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
    }

}
