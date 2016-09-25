// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


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
            return (CommandParameter)Utility.Json.Clone(this, this.GetType());
        }

        public string ToJson()
        {
            return Utility.Json.Serialize(this, this.GetType());
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
        [DispName("パラメータ共有")]
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
            return ModelContext.CommandTable?[CommandType].Parameter;
        }
    }


    /// <summary>
    /// 指定ページ数移動コマンド用パラメータ
    /// </summary>
    public class MoveSizePageCommandParameter : CommandParameter
    {
        [DispName("移動ページ数")]
        public int Size
        {
            get { return _Size; }
            set { _Size = NVUtility.Clamp(value, 0, 1000); }
        }
        private int _Size;
    }

    /// <summary>
    /// スケールモードトグル用設定
    /// </summary>
    [DataContract]
    public class ToggleStretchModeCommandParameter : CommandParameter
    {
        // 表示名
        [DataMember]
        [DispName(PageStretchModeExtension.PageStretchMode_Inside)]
        public bool IsEnableInside
        {
            get { return StretchModes[PageStretchMode.Inside]; }
            set { StretchModes[PageStretchMode.Inside] = value; }
        }

        [DataMember]
        [DispName(PageStretchModeExtension.PageStretchMode_Outside)]
        public bool IsEnableOutside
        {
            get { return StretchModes[PageStretchMode.Outside]; }
            set { StretchModes[PageStretchMode.Outside] = value; }
        }

        [DataMember]
        [DispName(PageStretchModeExtension.PageStretchMode_Uniform)]
        public bool IsEnableUniform
        {
            get { return StretchModes[PageStretchMode.Uniform]; }
            set { StretchModes[PageStretchMode.Uniform] = value; }
        }

        [DataMember]
        [DispName(PageStretchModeExtension.PageStretchMode_UniformToFill)]
        public bool IsEnableUniformToFill
        {
            get { return StretchModes[PageStretchMode.UniformToFill]; }
            set { StretchModes[PageStretchMode.UniformToFill] = value; }
        }

        [DataMember]
        [DispName(PageStretchModeExtension.PageStretchMode_UniformToSize)]
        public bool IsEnableUniformToSize
        {
            get { return StretchModes[PageStretchMode.UniformToSize]; }
            set { StretchModes[PageStretchMode.UniformToSize] = value; }
        }

        [DataMember]
        [DispName(PageStretchModeExtension.PageStretchMode_UniformToVertical)]
        public bool IsEnableUniformToVertical
        {
            get { return StretchModes[PageStretchMode.UniformToVertical]; }
            set { StretchModes[PageStretchMode.UniformToVertical] = value; }
        }


        //
        private Dictionary<PageStretchMode, bool> _StrechModes;
        public Dictionary<PageStretchMode, bool> StretchModes
        {
            get
            {
                if (_StrechModes == null)
                {
                    _StrechModes = Enum.GetValues(typeof(PageStretchMode)).Cast<PageStretchMode>().ToDictionary(e => e, e => true);
                }
                return _StrechModes;
            }
        }
    }


    /// <summary>
    /// スケールモード用設定
    /// </summary>
    public class StretchModeCommandParameter : CommandParameter
    {
        // 属性に説明文
        [DispName("オリジナルサイズとの切り替え", Tips = "既に指定のスケールモードの場合、オリジナルサイズにする")]
        public bool IsToggle { get; set; }
    }

    /// <summary>
    /// 回転コマンド用パラメータ
    /// </summary>
    public class ViewRotateCommandParameter : CommandParameter
    {
        // 属性に説明文
        [DispName("回転角度", Tips = "一度に回転する角度(0-180)")]
        public int Angle
        {
            get { return _Angle; }
            set { _Angle = NVUtility.Clamp(value, 0, 180); }
        }
        private int _Angle;
    }

}
