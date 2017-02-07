// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Preference テーブル
    /// </summary>
    public class Preference
    {
        /// <summary>
        /// Properties
        /// </summary>

        [DataMember, DefaultValue(true)]
        [PropertyMember()]
        public bool _configure_enabled { get; set; }

        [DataMember, DefaultValue(2)]
        [PropertyMember("画像読み込みに使用するスレッド数")]
        public int loader_thread_size { get; set; }

        [DataMember, DefaultValue(1.0)]
        [PropertyMember("横長画像を判定するための縦横比(横 / 縦)", Tips = "「横長ページを分割する」で使用されます")]
        public double view_image_wideratio { get; set; }

        [DataMember, DefaultValue(false)]
        [PropertyMember("履歴、ブックマーク、ページマークを保存しない", Tips = "履歴、ブックマーク、ページマークの情報がファイルに一切保存されなくなります")]
        public bool userdata_save_disable { get; set; }

        [DataMember, DefaultValue(30.0)]
        [PropertyMember("マウスジェスチャー判定の最小移動距離(X)", Tips = "この距離(pixel)移動して初めてジェスチャー開始と判定されます")]
        public double input_gesture_minimumdistance_x { get; set; }

        [DataMember, DefaultValue(30.0)]
        [PropertyMember("マウスジェスチャー判定の最小移動距離(Y)", Tips = "この距離(pixel)移動して初めてジェスチャー開始と判定されます")]
        public double input_gesture_minimumdistance_y { get; set; }

        [DataMember, DefaultValue(1.0)]
        [PropertyMember("パネルが自動的に消えるまでの時間(秒)")]
        public double panel_autohide_delaytime { get; set; }

        [DataMember, DefaultValue("")]
        [PropertyPath(Name = "7z.dll(32bit)の場所", Tips = "別の7z.dllを使用したい場合に設定します。反映には再起動が必要です")]
        public string loader_archiver_7z_dllpath { get; set; }

        [DataMember, DefaultValue(".7z;.rar;.lzh")]
        [PropertyMember("7z.dllで展開する圧縮ファイルの拡張子", Tips = ";(セミコロン)区切りでサポートする拡張子を羅列します。\n拡張子は .zip のように指定します")]
        public string loader_archiver_7z_supprtfiletypes { get; set; }

        [DataMember, DefaultValue("__MACOSX;.DS_Store")]
        [PropertyMember("ページ除外パス", Tips = ";(セミコロン)区切りで除外するパス名を羅列します。「サポート外ファイルもページに含める」設定では無効です")]
        public string loader_archiver_exclude { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public PropertyDocument Document { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        public Preference()
        {
            Document = PropertyDocument.Create(this);
        }

        /// <summary>
        /// 全ての設定値を初期化
        /// </summary>
        public void Reset()
        {
            foreach (var item in Document.Elements.OfType<PropertyMemberElement>())
            {
                item.ResetValue();
            }
        }



        /// <summary>
        /// 旧設定(Configurationファイル)のパラメータとの対応表
        /// </summary>
        private Dictionary<string, string> _configurationAppSettingTable = new Dictionary<string, string>()
        {
            ["GestureMinimumDistanceX"] = "input_gesture_minimumdistance_x",
            ["GestureMinimumDistanceY"] = "input_gesture_minimumdistance_y",
            ["PanelHideDelayTime"] = "panel_autohide_delaytime",
            ["SevenZipSupportFileType"] = "loader_archiver_7z_supprtfiletypes",
            ["ThreadSize"] = "loader_thread_size",
            ["WideRatio"] = "view_image_wideratio",
        };

        /// <summary>
        /// 旧設定(Configureファイル)からの読込。互換用
        /// </summary>
        public void LoadFromConfiguration()
        {
            // Configureファイルからの読込は最初の１度だけ
            if (!_configure_enabled) return;
            _configure_enabled = false;

            // configureファイルから読込
            foreach (var pair in _configurationAppSettingTable)
            {
                string value = System.Configuration.ConfigurationManager.AppSettings.Get(pair.Key);
                if (value != null)
                {
                    Document.GetPropertyMember(pair.Value)?.SetValue(value);
                }
            }
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public Dictionary<string, string> Items { get; set; }

            private void Constructor()
            {
                Items = new Dictionary<string, string>();
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }
        }

        /// <summary>
        /// Memento作成
        /// </summary>
        /// <returns></returns>
        public Memento CreateMemento()
        {
            var memento = new Memento();

            foreach (var item in Document.Elements.OfType<PropertyMemberElement>())
            {
                if (item.HasCustomValue)
                {
                    var key = item.Path.Replace('_', '.');
                    var value = item.GetValue().ToString();
                    memento.Items.Add(key, value);
                }
            }

            return memento;
        }

        /// <summary>
        /// Memento適用
        /// </summary>
        /// <param name="memento"></param>
        public void Restore(Memento memento)
        {
            this.Reset();

            foreach (var item in memento.Items)
            {
                try
                {
                    var path = item.Key.Replace('.', '_');
                    var element = Document.GetPropertyMember(path);
                    if (element != null)
                    {
                        element.SetValueFromString(item.Value);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            LoadFromConfiguration();
        }

        #endregion
    }
}
