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
using NeeView.Windows.Controls;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// コマンドパラメータ（基底）
    /// </summary>
    [DataContract]
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

        #endregion
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

        public override bool MemberwiseEquals(CommandParameter other)
        {
            var target = other as ShareCommandParameter;
            if (target == null) return false;
            return this == target || (this.CommandType == target.CommandType);
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
            return this == target || (
                this.IsLoop == target.IsLoop &&
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
            return this == target || (
                this.IsNScroll == target.IsNScroll &&
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
        [DataMember, PropertyPath("@ParamCommandParameterExportDefaultFolder", Tips = "ParamCommandParameterExportDefaultFolderTips", FileDialogType = FileDialogType.Directory)]
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
            return this == target || (
                this.Mode == target.Mode &&
                this.HasBackground == target.HasBackground &&
                this.ExportFolder == target.ExportFolder &&
                this.FileNameMode == target.FileNameMode &&
                this.FileFormat == target.FileFormat &&
                this.QualityLevel == target.QualityLevel);
        }
    }
}
