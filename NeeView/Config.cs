// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// アプリ全体の設定
    /// </summary>
    public class Config
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Config()
        {
        }

        // DPI倍率
        public Point DpiScaleFactor { get; private set; } = new Point(1, 1);

        // DPIのXY比率が等しい？
        public bool IsDpiSquare { get; private set; } = false;

        // DPI設定
        public void UpdateDpiScaleFactor(System.Windows.Media.Visual visual)
        {
            var dpiScaleFactor = DragExtensions.WPFUtil.GetDpiScaleFactor(visual);
            DpiScaleFactor = dpiScaleFactor;
            IsDpiSquare = DpiScaleFactor.X == DpiScaleFactor.Y;
        }
    }
}
