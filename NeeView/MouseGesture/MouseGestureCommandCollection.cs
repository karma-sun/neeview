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
        Dictionary<string, RoutedUICommand> _Commands;

        // 初期化
        public MouseGestureCommandCollection()
        {
            _Commands = new Dictionary<string, RoutedUICommand>();
        }

        // 辞書クリア
        public void Clear()
        {
            _Commands.Clear();
        }

        // コマンド追加
        public void Add(string gestureText, RoutedUICommand command)
        {
            _Commands[gestureText] = command;
        }

        // ジェスチャーシーケンスからコマンドを取得
        public RoutedUICommand GetCommand(MouseGestureSequence gesture)
        {
            string key = gesture.ToString();

            if (_Commands.ContainsKey(key))
            {
                return _Commands[key];
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
            if (_Commands.ContainsKey(gestureText))
            {
                if (_Commands[gestureText].CanExecute(null, null))
                {
                    _Commands[gestureText].Execute(null, null);
                }
            }
        }
    }

}
