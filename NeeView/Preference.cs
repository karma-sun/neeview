// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Preference項目
    /// </summary>
    public class PreferenceElement
    {
        /// <summary>
        /// キー
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 説明文
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// 初期値
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// ユーザ設定値。ない場合はnull
        /// </summary>
        public object Custom { get; set; }

        /// <summary>
        /// 現在値
        /// </summary>
        public object Value
        {
            get { return Custom ?? Default; }
            set { Custom = object.Equals(Default, value) ? null : value; }
        }

        /// <summary>
        /// ユーザ設定値の存在チェック
        /// </summary>
        public bool HasCustomValue => Custom != null;

        /// <summary>
        /// 値の型
        /// </summary>
        /// <returns></returns>
        public Type GetValueType()
        {
            return Default.GetType();
        }

        /// <summary>
        /// 値の型名
        /// </summary>
        /// <returns></returns>
        public string GetValueTypeString()
        {
            switch (Type.GetTypeCode(GetValueType()))
            {
                case TypeCode.String: return "文字列";
                case TypeCode.Boolean: return "真偽値";
                case TypeCode.Int32: return "整数値";
                case TypeCode.Double: return "実数値";
                default: return "???";
            }
        }

        /// <summary>
        /// 値の設定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void Set<T>(T value)
        {
            if (typeof(T) != GetValueType()) throw new InvalidOperationException("invalid value type");
            Value = value;
        }

        /// <summary>
        /// 値の取得
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>()
        {
            if (typeof(T) != GetValueType()) throw new InvalidOperationException("invalid value type");
            return (T)Value;
        }

        /// <summary>
        /// 文字列として取得
        /// </summary>
        public string String => Get<string>();

        /// <summary>
        /// 真偽値として取得
        /// </summary>
        public bool Boolean => Get<bool>();

        /// <summary>
        /// 整数値として取得
        /// </summary>
        public int Integer => Get<int>();

        /// <summary>
        /// 実数値として取得
        /// </summary>
        public double Double => Get<double>();
        
        /// <summary>
        /// 値を初期値に戻す
        /// </summary>
        public void Reset()
        {
            Custom = null;
        }

        /// <summary>
        /// 値を設定。文字列からパースする
        /// </summary>
        /// <param name="value"></param>
        public void SetParseValue(string value)
        {
            switch (Type.GetTypeCode(this.GetValueType()))
            {
                case TypeCode.String:
                    this.Value = value;
                    break;
                case TypeCode.Boolean:
                    this.Value = bool.Parse(value);
                    break;
                case TypeCode.Int32:
                    this.Value = int.Parse(value);
                    break;
                case TypeCode.Double:
                    this.Value = double.Parse(value);
                    break;
                default:
                    throw new NotSupportedException("not support type: " + this.GetValueType().ToString());
            }
        }
    }

    /// <summary>
    /// Preference テーブル
    /// </summary>
    public class Preference
    {
        /// <summary>
        /// 初期値リスト
        /// </summary>
        private List<PreferenceElement> Items = new List<PreferenceElement>()
        {
            new PreferenceElement()
            {
                Key = ".configure.enabled",
                Note = "load param from configure file (for old version)",
                Default = true,
            },

            new PreferenceElement()
            {
                Key = "input.gesture.minimumdistance.x",
                Note = "マウスジェスチャー判定の最小移動距離(X)",
                Default = 30.0
            },

            new PreferenceElement()
            {
                Key = "input.gesture.minimumdistance.y",
                Note = "マウスジェスチャー判定の最小移動距離(Y)",
                Default = 30.0
            },

            new PreferenceElement()
            {
                Key = "panel.autohide.delaytime",
                Note = "パネルが自動的に消えるまでの時間(秒)",
                Default = 1.0
            },

            new PreferenceElement()
            {
                Key = "loader.archiver.7z.supprtfiletypes",
                Note = "7z.dllで展開する圧縮ファイルの拡張子",
                Default = ".7z;.rar;.lzh"
            },

             new PreferenceElement()
            {
                Key = "loader.thread.size",
                Note = "画像読み込みに使用するスレッド数",
                Default = 2
            },

            new PreferenceElement()
            {
                Key = "view.image.wideratio",
                Note = "横長画像を判定するための縦横比(横/縦)。\n「横長ページを分割する」で使用されます",
                Default = 1.0
            }
        };

        /// <summary>
        /// 設定値辞書
        /// </summary>
        public Dictionary<string, PreferenceElement> Dictionary { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Preference()
        {
            Dictionary = Items.OrderBy(e => e.Key).ToDictionary(e => e.Key);
        }

        /// <summary>
        /// 全ての設定値を初期化
        /// </summary>
        public void Reset()
        {
            foreach (var item in Dictionary.Values)
            {
                item.Reset();
            }
        }

        /// <summary>
        /// インテグザ
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public PreferenceElement this[string key]
        {
            get { return Dictionary[key]; }
        }


        /// <summary>
        /// 値を設定（文字列をパース）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string key, string value)
        {
            if (!Dictionary.ContainsKey(key)) throw new ArgumentOutOfRangeException(nameof(key), "no support key");

            var item = Dictionary[key];
            item.SetParseValue(value);
        }


        /// <summary>
        /// 旧設定(Configurationファイル)のパラメータとの対応表
        /// </summary>
        private Dictionary<string, string> _ConfigurationAppSettingTable = new Dictionary<string, string>()
        {
            ["GestureMinimumDistanceX"] = "input.gesture.minimumdistance.x",
            ["GestureMinimumDistanceY"] = "input.gesture.minimumdistance.y",
            ["PanelHideDelayTime"] = "panel.autohide.delaytime",
            ["SevenZipSupportFileType"] = "loader.archiver.7z.supprtfiletypes",
            ["ThreadSize"] = "loader.thread.size",
            ["WideRatio"] = "view.image.wideratio",
        };

        /// <summary>
        /// 旧設定(Configureファイル)からの読込。互換用
        /// </summary>
        public void LoadFromConfiguration()
        {
            // Configureファイルからの読込は最初の１度だけ
            if (!Dictionary[".configure.enabled"].Boolean) return;
            Dictionary[".configure.enabled"].Set(false);

            // configureファイルから読込
            foreach (var pair in _ConfigurationAppSettingTable)
            {
                string value = System.Configuration.ConfigurationManager.AppSettings.Get(pair.Key);
                if (value != null)
                {
                    SetValue(pair.Value, value);
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

            foreach (var item in Dictionary.Values)
            {
                if (item.HasCustomValue)
                {
                    memento.Items.Add(item.Key, item.Value.ToString());
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
                    this.SetValue(item.Key, item.Value);
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
