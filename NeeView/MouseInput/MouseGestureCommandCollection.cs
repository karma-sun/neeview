// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// マウスジェスチャーシーケンスとコマンドの対応テーブル
    /// </summary>
    public class MouseGestureCommandCollection
    {
        public static MouseGestureCommandCollection Current { get; } = new MouseGestureCommandCollection();

        /// <summary>
        /// シーケンスとコマンドの対応辞書
        /// </summary>
        private Dictionary<string, RoutedUICommand> _commands;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public MouseGestureCommandCollection()
        {
            _commands = new Dictionary<string, RoutedUICommand>();
        }

        /// <summary>
        /// 辞書クリア
        /// </summary>
        public void Clear()
        {
            _commands.Clear();
        }

        /// <summary>
        /// コマンド追加
        /// </summary>
        /// <param name="gestureText"></param>
        /// <param name="command"></param>
        public void Add(string gestureText, RoutedUICommand command)
        {
            _commands[gestureText] = command;
        }

        /// <summary>
        /// ジェスチャーシーケンスからコマンドを取得
        /// </summary>
        /// <param name="gesture"></param>
        /// <returns></returns>
        public RoutedUICommand GetCommand(MouseGestureSequence gesture)
        {
            if (gesture == null) return null;

            string key = gesture.ToString();

            if (_commands.ContainsKey(key))
            {
                return _commands[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// ジェスチャーシーケンスからコマンドを実行
        /// </summary>
        /// <param name="gesture"></param>
        public void Execute(MouseGestureSequence gesture)
        {
            Execute(gesture.ToString());
        }

        /// <summary>
        /// ジェスチャー文字列からコマンドを実行
        /// </summary>
        /// <param name="gestureText"></param>
        public void Execute(string gestureText)
        {
            if (_commands.ContainsKey(gestureText))
            {
                if (_commands[gestureText].CanExecute(null, null))
                {
                    _commands[gestureText].Execute(null, null);
                }
            }
        }


        /// <summary>
        /// マウスジェスチャー通知
        /// </summary>
        public void ShowProgressed(MouseGestureSequence sequence)
        {
            var gesture = sequence.ToDispString();
            var commandName = this.GetCommand(sequence)?.Text;

            if (string.IsNullOrEmpty(gesture) && string.IsNullOrEmpty(commandName)) return;

            InfoMessage.Current.SetMessage(
                InfoMessageType.Gesture,
                ((commandName != null) ? commandName + "\n" : "") + gesture,
                gesture + ((commandName != null) ? " " + commandName : ""));
        }

    }
}
