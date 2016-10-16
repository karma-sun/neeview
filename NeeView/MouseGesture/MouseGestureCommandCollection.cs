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
        // シーケンスとコマンドの対応辞書
        private Dictionary<string, RoutedUICommand> _commands;

        // 初期化
        public MouseGestureCommandCollection()
        {
            _commands = new Dictionary<string, RoutedUICommand>();
        }

        // 辞書クリア
        public void Clear()
        {
            _commands.Clear();
        }

        // コマンド追加
        public void Add(string gestureText, RoutedUICommand command)
        {
            _commands[gestureText] = command;
        }

        // ジェスチャーシーケンスからコマンドを取得
        public RoutedUICommand GetCommand(MouseGestureSequence gesture)
        {
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

        // ジェスチャーシーケンスからコマンドを実行
        public void Execute(MouseGestureSequence gesture)
        {
            Execute(gesture.ToString());
        }

        // セスチャー文字列からコマンドを実行
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
    }
}
