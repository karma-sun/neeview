using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
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
    public class RoutedCommandTable
    {
        public static RoutedCommandTable Current { get; private set; }

        #region Fields

        private MainWindow _window;
        private CommandTable _commandTable;
        private Dictionary<Key, bool> _usedKeyMap;
        private bool _isDarty;
        private List<EventHandler<KeyEventArgs>> _imeKeyHandlers = new List<EventHandler<KeyEventArgs>>();

        #endregion

        #region Constructors

        public RoutedCommandTable(MainWindow window, CommandTable commandTable)
        {
            Current = this;

            _window = window;
            _commandTable = commandTable;

            // RoutedCommand作成
            foreach (CommandType type in Enum.GetValues(typeof(CommandType)))
            {
                Commands.Add(type, new RoutedUICommand(commandTable[type].Text, type.ToString(), typeof(MainWindow)));
            }

            // コマンド変更でショートカット変更
            _commandTable.Changed += CommandTable_Changed;
        }

        #endregion

        #region Events

        /// <summary>
        /// コマンドテーブルが更新されたときのイベント
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// コマンドが実行されたときのイベント
        /// </summary>
        public event EventHandler<CommandExecutedEventArgs> CommandExecuted;

        #endregion

        #region Properties

        /// <summary>
        /// コマンド辞書
        /// </summary>
        public Dictionary<CommandType, RoutedUICommand> Commands { get; set; } = new Dictionary<CommandType, RoutedUICommand>();

        #endregion

        #region Methods

        //
        private void CommandTable_Changed(object sender, CommandChangedEventArgs e)
        {
            _isDarty = true;

            if (!e.OnHold)
            {
                InitializeInputGestures();
            }
        }

        //
        public void SetDarty()
        {
            _isDarty = true;
        }

        // InputGesture設定
        public void InitializeInputGestures()
        {
            if (!_isDarty) return;
            _isDarty = false;

            // Touch
            var touch = TouchInput.Current;

            touch.ClearTouchEventHandler();

            foreach (var command in this.Commands)
            {
                var touchGestures = CommandTable.Current[command.Key].GetTouchGestureCollection();
                foreach (var gesture in touchGestures)
                {
                    touch.TouchGestureChanged += (s, x) =>
                    {
                        if (command.Key == CommandType.TouchEmulate) return;

                        if (!x.RoutedEventArgs.Handled && x.Gesture == gesture)
                        {
                            command.Value.Execute(null, _window);
                            x.RoutedEventArgs.Handled = true;
                        }
                    };
                }
            }

            // Mouse / Keyboard
            var mouse = MouseInput.Current;

            mouse.ClearMouseEventHandler();

            MouseGestureCommandCollection.Current.Clear();

            var imeKeyHandlers = new List<EventHandler<KeyEventArgs>>();
            var mouseNormalHandlers = new List<EventHandler<MouseButtonEventArgs>>();
            var mouseExtraHndlers = new List<EventHandler<MouseButtonEventArgs>>();

            foreach (var command in this.Commands)
            {
                command.Value.InputGestures.Clear();
                var inputGestures = CommandTable.Current[command.Key].GetInputGestureCollection();
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
                        mouse.MouseWheelChanged += (s, x) => { if (!x.Handled && gesture.Matches(this, x)) { WheelCommandExecute(command.Value, x); } };
                    }
                    else
                    {
                        if (gesture.HasImeKey())
                        {
                            imeKeyHandlers.Add((s, x) => InputGestureCommandExecute(s, x, gesture, command.Value));
                        }
                        command.Value.InputGestures.Add(gesture);
                    }
                }

                // mouse gesture
                var mouseGesture = CommandTable.Current[command.Key].MouseGesture;
                if (mouseGesture != null)
                {
                    MouseGestureCommandCollection.Current.Add(mouseGesture, command.Key);
                }
            }

            _imeKeyHandlers = imeKeyHandlers;

            // 拡張マウス入力から先に処理を行う
            foreach (var lambda in mouseExtraHndlers.Concat(mouseNormalHandlers))
            {
                mouse.MouseButtonChanged += lambda;
            }

            InitialzeUsedKeyMap();

            Changed?.Invoke(this, null);
        }

        // コマンドで使用されているキーマップ生成
        private void InitialzeUsedKeyMap()
        {
            var map = Enum.GetValues(typeof(Key)).Cast<Key>().Distinct().ToDictionary(e => e, e => false);

            foreach (var command in this.Commands)
            {
                var inputGestures = CommandTable.Current[command.Key].GetInputGestureCollection();
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
                command.Execute(null, _window);
                CommandExecuted?.Invoke(this, new CommandExecutedEventArgs() { Gesture = gesture });
                if (x.RoutedEvent != null)
                {
                    x.Handled = true;
                }
            }
        }

        // ホイールの回転数に応じたコマンド実行
        private void WheelCommandExecute(RoutedUICommand command, MouseWheelEventArgs arg)
        {
            int turn = MouseInputHelper.DeltaCount(arg);

            // Debug.WriteLine($"WheelCommand: {turn}({arg.Delta})");
            var param = new CommandParameterArgs(null, CommandTable.Current.IsReversePageMoveWheel);
            for (int i = 0; i < turn; i++)
            {
                command.Execute(param, _window);
            }
        }

        // コマンド実行 
        // CommandTableを純粋なコマンド定義のみにするため、コマンド実行に伴う処理はここで定義している
        public void Execute(object sender, ExecutedRoutedEventArgs e, CommandType type)
        {
            var param = CommandParameterArgs.Create(e.Parameter) ?? CommandParameterArgs.Null;
            var allowFlip = param.AllowFlip && param.Parameter != MenuCommandTag.Tag; // メニューからの操作ではページ方向によるコマンドの入れ替えをしない
            var command = _commandTable[GetFixedCommandType(type, allowFlip)];

            // 通知
            if (command.IsShowMessage)
            {
                string message = command.ExecuteMessage(param.Parameter);
                InfoMessage.Current.SetMessage(InfoMessageType.Command, message);
            }

            // 実行
            command.Execute(e.Source, e);
        }

        // スライダー方向によって移動コマンドを入れ替える
        public CommandType GetFixedCommandType(CommandType commandType, bool allowFlip)
        {
            if (allowFlip && CommandTable.Current.IsReversePageMove && MainWindowModel.Current.IsLeftToRightSlider())
            {
                var command = _commandTable[commandType];
                if (command.PairPartner != CommandType.None)
                {
                    ////Debug.WriteLine($"SwapCommand: {commandType} to {command.PairPartner}");
                    return command.PairPartner;
                }
                else
                {
                    return commandType;
                }
            }
            else
            {
                return commandType;
            }
        }

        //
        public CommandElement GetFixedCommandElement(CommandType commandType, bool allowRecursive)
        {
            _commandTable.TryGetValue(GetFixedCommandType(commandType, allowRecursive), out CommandElement command);
            return command;
        }

        //
        public RoutedUICommand GetFixedRoutedCommand(CommandType commandType, bool allowRecursive)
        {
            this.Commands.TryGetValue(GetFixedCommandType(commandType, allowRecursive), out RoutedUICommand command);
            return command;
        }

        #endregion

        #region Memento
        // compatible before ver.23
        [Obsolete, DataContract]
        public class Memento
        {
            [Obsolete, DataMember(EmitDefaultValue = false)]
            public ShowMessageStyle CommandShowMessageStyle { get; set; }
        }

#pragma warning disable CS0612

        public void RestoreCompatible(Memento memento)
        {
            if (memento == null) return;
            InfoMessage.Current.CommandShowMessageStyle = memento.CommandShowMessageStyle;
        }

#pragma warning restore CS0612


        #endregion
    }

    /// <summary>
    /// コマンドパラメータ引数管理用
    /// </summary>
    public class CommandParameterArgs
    {
        public CommandParameterArgs(object param)
        {
            Parameter = param;
            AllowFlip = true;
        }

        public CommandParameterArgs(object param, bool allowRecursive)
        {
            Parameter = param;
            AllowFlip = allowRecursive;
        }


        /// <summary>
        /// 標準パラメータ
        /// </summary>
        public static CommandParameterArgs Null { get; } = new CommandParameterArgs(null);

        /// <summary>
        /// パラメータ本体
        /// </summary>
        public object Parameter { get; set; }

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
