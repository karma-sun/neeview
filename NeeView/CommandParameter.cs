// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace NeeView
{
    /// <summary>
    /// コマンドパラメータ（基底）
    /// </summary>
    public class CommandParameter
    {
        public CommandParameter Clone()
        {
            return (CommandParameter)Utility.Json.Clone(this, this.GetType());
        }

        internal string ToJson()
        {
            return Utility.Json.Serialize(this, this.GetType());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SetStretchModeCommandParameter : CommandParameter
    {
        // 属性に説明文
        [DispName("オリジナルサイズとの切り替え", Tips = "既に指定のスケールモードの場合、オリジナルサイズにする")]
        public bool IsToggle { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ViewRotateCommandParameter : CommandParameter
    {
        // 属性に説明文
        [DispName("回転する角度", Tips = "一度に回転する角度(0-180)")]
        public int Angle
        {
            get { return _Angle; }
            set { _Angle = NVUtility.Clamp(value, 0, 180); }
        }
        private int _Angle;
    }

}
