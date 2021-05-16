using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class CommandExecutedEventArgs : EventArgs
    {
        public InputGesture Gesture { get; set; }
    }


    /// <summary>
    /// コマンド集 ： RoutedCommand
    /// </summary>
    public class RoutedCommandTable : IDisposable
    {
        static RoutedCommandTable() => Current = new RoutedCommandTable();
        public static RoutedCommandTable Current { get; }


        private Dictionary<Key, bool> _usedKeyMap;
        private bool _isDarty = true;
        private List<EventHandler<KeyEventArgs>> _imeKeyHandlers = new List<EventHandler<KeyEventArgs>>();
        private MouseWheelDelta _mouseWheelDelta = new MouseWheelDelta();
        private List<TouchInput> _touchInputCollection = new List<TouchInput>();
        private List<MouseInput> _mouseInputCollection = new List<MouseInput>();
        private bool _disposedValue;

        private RoutedCommandTable()
        {
            // RoutedCommand作成
            foreach (var command in CommandTable.Current)
            {
                Commands.Add(command.Key, new RoutedUICommand(command.Value.Text, command.Key, typeof(MainWindow)));
            }

            // コマンド変更でショートカット変更
            CommandTable.Current.Changed += CommandTable_Changed;

            UpdateInputGestures();
        }


        /// <summary>
        /// コマンドテーブルが更新されたときのイベント
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// コマンドが実行されたときのイベント
        /// </summary>
        public event EventHandler<CommandExecutedEventArgs> CommandExecuted;

        /// <summary>
        /// コマンド辞書
        /// </summary>
        public Dictionary<string, RoutedUICommand> Commands { get; set; } = new Dictionary<string, RoutedUICommand>();



        private void CommandTable_Changed(object sender, CommandChangedEventArgs e)
        {
            _isDarty = true;

            if (!e.OnHold)
            {
                UpdateInputGestures();
            }
        }

        public void SetDarty()
        {
            _isDarty = true;
        }

        public void UpdateRoutedCommand()
        {
            var oldies = Commands.Keys
                .ToList();

            var newers = CommandTable.Current.Keys
                .ToList();

            foreach (var name in oldies.Except(newers))
            {
                Commands.Remove(name);
            }

            foreach (var name in newers.Except(oldies))
            {
                var command = CommandTable.Current.GetElement(name) ?? throw new InvalidOperationException();
                Commands.Add(name, new RoutedUICommand(command.Text, name, typeof(MainWindow)));
            }
        }




        public void AddTouchInput(TouchInput touchInput)
        {
            _touchInputCollection.Add(touchInput);
            UpdateTouchInputGestures(touchInput);
        }

        public void AddMouseInput(MouseInput mouseInput)
        {
            _mouseInputCollection.Add(mouseInput);
            UpdateMouseInputGestures(mouseInput);
        }


        // InputGesture設定
        public void UpdateInputGestures()
        {
            if (!_isDarty) return;
            _isDarty = false;

            if (_disposedValue) return;

            UpdateRoutedCommand();
            ClearRoutedCommandInputGestures();

            UpdateMouseDragGestures();

            foreach (var touchInput in _touchInputCollection)
            {
                UpdateTouchInputGestures(touchInput);
            }

            foreach (var mouseInput in _mouseInputCollection)
            {
                UpdateMouseInputGestures(mouseInput);
            }

            UpdateKeyInputGestures();

            Changed?.Invoke(this, null);
        }


        private void ClearRoutedCommandInputGestures()
        {
            foreach (var command in this.Commands)
            {
                command.Value.InputGestures.Clear();
            }
        }

        private void UpdateMouseDragGestures()
        {
            MouseGestureCommandCollection.Current.Clear();

            foreach (var command in this.Commands)
            {
                var mouseGesture = CommandTable.Current.GetElement(command.Key).MouseGesture;
                if (mouseGesture != null)
                {
                    MouseGestureCommandCollection.Current.Add(mouseGesture, command.Key);
                }
            }
        }

        private void UpdateTouchInputGestures(TouchInput touch)
        {
            touch.ClearTouchEventHandler();

            foreach (var command in this.Commands)
            {
                var touchGestures = CommandTable.Current.GetElement(command.Key).GetTouchGestureCollection();
                foreach (var gesture in touchGestures)
                {
                    touch.TouchGestureChanged += (s, x) =>
                    {
                        if (command.Key == "TouchEmulate") return;

                        if (!x.Handled && x.Gesture == gesture)
                        {
                            command.Value.Execute(null, (s as IInputElement) ?? MainWindow.Current);
                            x.Handled = true;
                        }
                    };
                }
            }
        }

        private void UpdateMouseInputGestures(MouseInput mouse)
        {
            mouse.ClearMouseEventHandler();

            var mouseNormalHandlers = new List<EventHandler<MouseButtonEventArgs>>();
            var mouseExtraHndlers = new List<EventHandler<MouseButtonEventArgs>>();

            foreach (var command in this.Commands)
            {
                var inputGestures = CommandTable.Current.GetElement(command.Key).GetInputGestureCollection();
                foreach (var gesture in inputGestures)
                {
                    if (gesture is MouseGesture mouseClick)
                    {
                        mouseNormalHandlers.Add((s, x) => InputGestureCommandExecute(s, x, gesture, command.Value));
                    }
                    else if (gesture is MouseExGesture)
                    {
                        mouseExtraHndlers.Add((s, x) => InputGestureCommandExecute(s, x, gesture, command.Value));
                    }
                    else if (gesture is MouseWheelGesture)
                    {
                        mouse.MouseWheelChanged += (s, x) => { if (!x.Handled && gesture.Matches(this, x)) { WheelCommandExecute(s, x, command.Value); } };
                    }
                    else if (gesture is MouseHorizontalWheelGesture)
                    {
                        mouse.MouseHorizontalWheelChanged += (s, x) => { if (!x.Handled && gesture.Matches(this, x)) { WheelCommandExecute(s, x, command.Value); } };
                    }
                }
            }

            // NOTE: 拡張マウス入力から先に処理を行う
            foreach (var lambda in mouseExtraHndlers.Concat(mouseNormalHandlers))
            {
                mouse.MouseButtonChanged += lambda;
            }
        }


        /// <summary>
        /// Initialize KeyInuput gestures
        /// </summary>
        private void UpdateKeyInputGestures()
        {
            var imeKeyHandlers = new List<EventHandler<KeyEventArgs>>();

            foreach (var command in this.Commands)
            {
                var inputGestures = CommandTable.Current.GetElement(command.Key).GetInputGestureCollection();
                foreach (var gesture in inputGestures.Where(e => e is KeyGesture || e is KeyExGesture))
                {
                    if (gesture.HasImeKey())
                    {
                        imeKeyHandlers.Add((s, x) => InputGestureCommandExecute(s, x, gesture, command.Value));
                    }
                    command.Value.InputGestures.Add(gesture);
                }
            }

            _imeKeyHandlers = imeKeyHandlers;

            UpdateUsedKeyMap();
        }


        // コマンドで使用されているキーマップ生成
        private void UpdateUsedKeyMap()
        {
            var map = Enum.GetValues(typeof(Key)).Cast<Key>().Distinct().ToDictionary(e => e, e => false);

            foreach (var command in this.Commands)
            {
                var inputGestures = CommandTable.Current.GetElement(command.Key).GetInputGestureCollection();
                foreach (var gesture in inputGestures)
                {
                    switch (gesture)
                    {
                        case KeyGesture keyGesture:
                            map[keyGesture.Key] = true;
                            break;
                        case KeyExGesture keyExGesture:
                            map[keyExGesture.Key] = true;
                            break;
                    }
                }
            }

            _usedKeyMap = map;
        }

        // コマンドで使用されているキー？
        public bool IsUsedKey(Key key)
        {
            return _usedKeyMap != null ? _usedKeyMap[key] : false;
        }

        // IMEキーコマンドを直接実行
        public void ExecuteImeKeyGestureCommand(object sender, KeyEventArgs args)
        {
            foreach (var handle in _imeKeyHandlers)
            {
                if (args.Handled) return;
                handle.Invoke(sender, args);
            }
        }

        // コマンドのジェスチャー判定と実行
        private void InputGestureCommandExecute(object sender, InputEventArgs x, InputGesture gesture, RoutedUICommand command)
        {
            if (!x.Handled && gesture.Matches(this, x))
            {
                command.Execute(null, (sender as IInputElement) ?? MainWindow.Current);
                CommandExecuted?.Invoke(this, new CommandExecutedEventArgs() { Gesture = gesture });
                if (x.RoutedEvent != null)
                {
                    x.Handled = true;
                }
            }
        }

        // ホイールの回転数に応じたコマンド実行
        private void WheelCommandExecute(object sender, MouseWheelEventArgs arg, RoutedUICommand command)
        {
            int turn = Math.Abs(_mouseWheelDelta.NotchCount(arg));
            if (turn == 0) return;

            // Debug.WriteLine($"WheelCommand: {turn}({arg.Delta})");
            var param = new CommandParameterArgs(null, Config.Current.Command.IsReversePageMoveWheel);
            for (int i = 0; i < turn; i++)
            {
                command.Execute(param, (sender as IInputElement) ?? MainWindow.Current);
            }
        }

        // コマンド実行 
        // CommandTableを純粋なコマンド定義のみにするため、コマンド実行に伴う処理はここで定義している
        public void Execute(object sender, string name, object parameter)
        {
            bool allowFlip = (parameter is CommandParameterArgs args)
                ? args.AllowFlip
                : (parameter != MenuCommandTag.Tag);

            var command = CommandTable.Current.GetElement(GetFixedCommandName(name, allowFlip));

            // 通知
            if (command.IsShowMessage)
            {
                string message = command.ExecuteMessage(sender, CommandArgs.Empty);
                if (message != null)
                {
                    InfoMessage.Current.SetMessage(InfoMessageType.Command, message);
                }
            }

            // 実行
            var option = (parameter is MenuCommandTag) ? CommandOption.ByMenu : CommandOption.None;
            command.Execute(sender, new CommandArgs(null, option));
        }

        // スライダー方向によって移動コマンドを入れ替える
        public string GetFixedCommandName(string name, bool allowFlip)
        {
            if (allowFlip && Config.Current.Command.IsReversePageMove && MainWindowModel.Current.IsLeftToRightSlider())
            {
                CommandTable.Current.TryGetValue(name, out var command);
                if (command != null && command.PairPartner != null)
                {
                    if (command.Parameter is ReversibleCommandParameter reversibleCommandParameter)
                    {
                        return reversibleCommandParameter.IsReverse ? command.PairPartner : name;
                    }
                    else
                    {
                        return command.PairPartner;
                    }
                }
                else
                {
                    return name;
                }
            }
            else
            {
                return name;
            }
        }

        public CommandElement GetFixedCommandElement(string commandName, bool allowRecursive)
        {
            CommandTable.Current.TryGetValue(GetFixedCommandName(commandName, allowRecursive), out CommandElement command);
            return command;
        }

        public RoutedUICommand GetFixedRoutedCommand(string commandName, bool allowRecursive)
        {
            this.Commands.TryGetValue(GetFixedCommandName(commandName, allowRecursive), out RoutedUICommand command);
            return command;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

    /// <summary>
    /// コマンドパラメータ引数管理用
    /// </summary>
    public class CommandParameterArgs
    {
        public CommandParameterArgs(object param)
        {
            ////Parameter = param;
            AllowFlip = true;
        }

        public CommandParameterArgs(object param, bool allowRecursive)
        {
            ////Parameter = param;
            AllowFlip = allowRecursive;
        }


        /// <summary>
        /// 標準パラメータ
        /// </summary>
        ////public static CommandParameterArgs Null { get; } = new CommandParameterArgs(null);

        /// <summary>
        /// パラメータ本体
        /// </summary>
        ////public object Parameter { get; set; }

        /// <summary>
        /// スライダー方向でのコマンド入れ替え許可
        /// </summary>
        public bool AllowFlip { get; set; }


        public static CommandParameterArgs Create(object param)
        {
            if (param is CommandParameterArgs parameterArgs)
            {
                return parameterArgs;
            }
            else
            {
                return new CommandParameterArgs(param);
            }
        }
    }
}
