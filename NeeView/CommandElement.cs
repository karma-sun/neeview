// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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

        // ショートカットキー
        public string ShortCutKey { get; set; }

        // マウスジェスチャー
        public string MouseGesture { get; set; }

        // コマンド実行時の通知フラグ
        public bool IsShowMessage { get; set; }

        // コマンド本体
        public Action<object> Execute { get; set; }

        // コマンド実行時表示デリゲート
        public Func<object, string> ExecuteMessage { get; set; }

        // コマンド実行可能判定
        public Func<bool> CanExecute { get; set; }


        // constructor
        public CommandElement()
        {
            IsShowMessage = true;
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
            [DataMember]
            public string ShortCutKey { get; set; }
            [DataMember]
            public string MouseGesture { get; set; }
            [DataMember]
            public bool IsShowMessage { get; set; }

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
            return memento;
        }

        //
        public void Restore(Memento element)
        {
            ShortCutKey = element.ShortCutKey;
            MouseGesture = element.MouseGesture;
            MouseGesture = element.MouseGesture;
        }

        #endregion
    }
}
