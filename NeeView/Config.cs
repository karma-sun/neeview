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
        // App.Configの設定
        public ConfigItem GestureMinimumDistanceX { get; private set; }
        public ConfigItem GestureMinimumDistanceY { get; private set; }
        public ConfigItem PanelHideDelayTime { get; private set; }
        public ConfigItem SevenZipSupportFileType { get; private set; }
        public ConfigItem ThreadSize { get; private set; }
        public ConfigItem WideRatio { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Config()
        {
            GestureMinimumDistanceX = new ConfigItem(nameof(GestureMinimumDistanceX), "30");
            GestureMinimumDistanceY = new ConfigItem(nameof(GestureMinimumDistanceY), "30");
            PanelHideDelayTime = new ConfigItem(nameof(PanelHideDelayTime), "1.0");
            SevenZipSupportFileType = new ConfigItem(nameof(SevenZipSupportFileType), ".7z;.rar;.lzh");
            ThreadSize = new ConfigItem(nameof(ThreadSize), "2");
            WideRatio = new ConfigItem(nameof(WideRatio), "1.0");
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

    /// <summary>
    /// App.Configのパラメータにアクセスする。
    /// 入力に合わない型で取得しようとした場合、デフォルト値を返す。
    /// デフォルト値も型に合わない場合は例外になる。
    /// </summary>
    public class ConfigItem
    {
        public string Name { get; set; }

        private string _DefaultValue;
        private string _Value;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name">パラメータ名</param>
        /// <param name="def">デフォルト値</param>
        public ConfigItem(string name, string def)
        {
            Name = name;
            _Value = ConfigurationManager.AppSettings.Get(Name) ?? _DefaultValue;
            _DefaultValue = def;
        }

        /// <summary>
        /// string として値を取得する
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _Value;
        }

        /// <summary>
        /// int として値を取得する
        /// </summary>
        /// <returns></returns>
        public int ToInt()
        {
            int value;
            if (int.TryParse(_Value, out value))
                return value;
            else
                return int.Parse(_DefaultValue);
        }

        /// <summary>
        /// double として値を取得する
        /// </summary>
        /// <returns></returns>
        public double ToDouble()
        {
            double value;
            if (double.TryParse(_Value, out value))
                return value;
            else
                return double.Parse(_DefaultValue);
        }
    }
}
