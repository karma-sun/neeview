// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// コマンド設定
    /// </summary>
    public class CommandElement
    {
        // コマンドのグループ
        public string Group { get; set; }

        // コマンド表示名
        public string Text { get; set; }

        // メニュー表示名
        private string _menuText;
        public string MenuText
        {
            get { return _menuText ?? Text; }
            set { _menuText = value; }
        }

        // ショートカットキー
        public string ShortCutKey { get; set; }

        // マウスジェスチャー
        public string MouseGesture { get; set; }

        // コマンド実行時の通知フラグ
        public bool IsShowMessage { get; set; }

        // コマンド本体
        public Action<object, object> Execute { get; set; }

        // コマンド実行時表示デリゲート
        public Func<object, string> ExecuteMessage { get; set; }

        // コマンド実行可能判定
        public Func<bool> CanExecute { get; set; }

        // フラグバインディング
        public Func<System.Windows.Data.Binding> CreateIsCheckedBinding { get; set; }


        // トグル候補
        public bool IsToggled { get; set; }

        // コマンド説明
        public string Note { get; set; }

        // コマンド説明をTips用文字列に変換
        public string NoteToTips()
        {
            return new Regex(@"<[\w/]+>").Replace(Note, "\"").Replace("。", "\n");
        }


        // コマンドパラメータ標準
        public CommandParameter DefaultParameter { get; set; }

        // コマンドパラメータ
        private CommandParameter _parameterRaw;

        // コマンドパラメータ(Raw)
        public CommandParameter ParameterRaw
        {
            get { return GetParameter(true); }
        }

        // コマンドパラメータ
        // 共有解決したパラメータ。通常はこちらを使用します
        public CommandParameter Parameter
        {
            get { return GetParameter(false); }
            set { SetParameter(value); }
        }

        //        
        public bool HasParameter => DefaultParameter != null;

        /// <summary>
        /// パラメータ取得
        /// </summary>
        /// <param name="isRaw"></param>
        /// <returns></returns>
        public CommandParameter GetParameter(bool isRaw)
        {
            if (_parameterRaw == null && DefaultParameter != null)
            {
                _parameterRaw = DefaultParameter.Clone();
            }

            if (isRaw)
            {
                return _parameterRaw;
            }
            else
            {
                return _parameterRaw?.Entity();
            }
        }

        /// <summary>
        /// パラメータ設定
        /// </summary>
        /// <param name="value"></param>
        private void SetParameter(CommandParameter value)
        {
            if (!DefaultParameter.IsReadOnly())
            {
                _parameterRaw = value;
            }
        }

        // constructor
        public CommandElement()
        {
            ExecuteMessage = e => Text;
        }

        // ショートカットキー を InputGestureのコレクションに変換
        public List<InputGesture> GetInputGestureCollection()
        {
            var list = new List<InputGesture>();
            if (ShortCutKey != null)
            {
                foreach (var key in ShortCutKey.Split(','))
                {
                    InputGesture inputGesture = InputGestureConverter.ConvertFromString(key);
                    if (inputGesture != null)
                    {
                        list.Add(inputGesture);
                    }
                }
            }

            return list;
        }


        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember(EmitDefaultValue = false)]
            public string ShortCutKey { get; set; }
            [DataMember(EmitDefaultValue = false)]
            public string MouseGesture { get; set; }
            [DataMember(EmitDefaultValue = false)]
            public bool IsShowMessage { get; set; }
            [DataMember(Order = 2, EmitDefaultValue = false)] // 廃止
            public bool IsToggled { get; set; }
            [DataMember(Order = 15, EmitDefaultValue = false)]
            public string Parameter { get; set; }


            //
            private void Constructor()
            {
            }

            //
            public Memento()
            {
                Constructor();
            }

            //
            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }

            //
            public Memento Clone()
            {
                return (Memento)MemberwiseClone();
            }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.ShortCutKey = ShortCutKey;
            memento.MouseGesture = MouseGesture;
            memento.IsShowMessage = IsShowMessage;

            if (HasParameter && !DefaultParameter.IsReadOnly())
            {
                var original = DefaultParameter.ToJson();
                var current = Parameter.ToJson();
                if (original != current)
                {
                    memento.Parameter = current;
                }
            }

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            ShortCutKey = memento.ShortCutKey;
            MouseGesture = memento.MouseGesture;
            IsShowMessage = memento.IsShowMessage;
            IsToggled = memento.IsToggled;

            if (HasParameter && memento.Parameter != null)
            {
                Parameter = (CommandParameter)Utility.Json.Deserialize(memento.Parameter, DefaultParameter.GetType());
            }
        }

        #endregion
    }
}
