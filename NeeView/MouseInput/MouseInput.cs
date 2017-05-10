// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// マウス情報のモデル.
/// ModelからはModelしかアクセス出来ないため。
/// </summary>
namespace NeeView
{
    public class MouseInput 
    {
        /// <summary>
        /// 一定距離カーソルが移動した
        /// </summary>
        public event EventHandler MouseMoved;

        /// <summary>
        /// 一定距離カーソルが移動したイベント発行
        /// </summary>
        public void RaiseMouseMoved()
        {
            MouseMoved?.Invoke(this, null);
        }
    }
}
