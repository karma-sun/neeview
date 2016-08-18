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
    [Flags]
    public enum CommandAttribute
    {
        None = 0,
        ToggleEditable = (1 << 0),
        ToggleLocked = (1 << 1)
    }

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
        private string _MenuText;
        public string MenuText
        {
            get { return _MenuText ?? Text; }
            set { _MenuText = value; }
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

        // 属性
        public CommandAttribute Attribute { get; set; }

        // constructor
        public CommandElement()
        {
            IsShowMessage = true;
            ExecuteMessage = e => Text;
            IsToggled = true;
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
            [DataMember]
            public string ShortCutKey { get; set; }
            [DataMember]
            public string MouseGesture { get; set; }
            [DataMember]
            public bool IsShowMessage { get; set; }
            [DataMember(Order = 2)]
            public bool IsToggled { get; set; }


            //
            private void Constructor()
            {
                IsToggled = true;
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
            memento.IsToggled = IsToggled;
            return memento;
        }

        //
        public void Restore(Memento element)
        {
            ShortCutKey = element.ShortCutKey;
            MouseGesture = element.MouseGesture;
            IsShowMessage = element.IsShowMessage;
            IsToggled = element.IsToggled;
        }

        #endregion
    }
}
