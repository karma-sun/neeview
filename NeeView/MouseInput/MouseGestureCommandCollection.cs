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
        static MouseGestureCommandCollection() => Current = new MouseGestureCommandCollection();
        public static MouseGestureCommandCollection Current { get; }

        /// <summary>
        /// シーケンスとコマンドの対応辞書
        /// </summary>
        private Dictionary<string, string> _commands;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public MouseGestureCommandCollection()
        {
            _commands = new Dictionary<string, string>();
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
        public void Add(string gestureText, string command)
        {
            _commands[gestureText] = command;
        }

        /// <summary>
        /// ジェスチャーシーケンスからコマンドを取得
        /// </summary>
        /// <param name="gesture"></param>
        /// <returns></returns>
        public string GetCommand(MouseGestureSequence gesture)
        {
            if (gesture == null || gesture.Count == 0) return "";

            string key = gesture.ToString();

            if (_commands.ContainsKey(key))
            {
                return _commands[key];
            }
            else
            {
                return "";
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
                var routedCommand = RoutedCommandTable.Current.Commands[_commands[gestureText]];
                if (routedCommand != null && routedCommand.CanExecute(null, null))
                {
                    routedCommand.Execute(null, null);
                }
            }
        }


        /// <summary>
        /// マウスジェスチャー通知
        /// </summary>
        public void ShowProgressed(MouseGestureSequence sequence)
        {
            var gesture = sequence.ToDispString();

            var commandName = GetCommand(sequence);
            var commandText = RoutedCommandTable.Current.GetFixedRoutedCommand(commandName, true)?.Text;

            if (string.IsNullOrEmpty(gesture) && string.IsNullOrEmpty(commandText)) return;

            InfoMessage.Current.SetMessage(
                InfoMessageType.Gesture,
                ((commandText != null) ? commandText + "\n" : "") + gesture,
                gesture + ((commandText != null) ? " " + commandText : ""));
        }

    }
}
