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
        static RoutedCommandTable() => Current = new RoutedCommandTable();
        public static RoutedCommandTable Current { get; }

        #region Fields

        private Dictionary<Key, bool> _usedKeyMap;
        private bool _isDarty;
        private List<EventHandler<KeyEventArgs>> _imeKeyHandlers = new List<EventHandler<KeyEventArgs>>();
        private MouseWheelDelta _mouseWheelDelta = new MouseWheelDelta();

        #endregion

        #region Constructors

        private RoutedCommandTable()
        {
            // RoutedCommand作成
            foreach (var command in CommandTable.Current)
            {
                Commands.Add(command.Key, new RoutedUICommand(command.Value.Text, command.Key, typeof(MainWindow)));
            }

            // コマンド変更でショートカット変更
            CommandTable.Current.Changed += CommandTable_Changed;
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
        public Dictionary<string, RoutedUICommand> Commands { get; set; } = new Dictionary<string, RoutedUICommand>();

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

        // Update RoutedCommand
        // スクリプトコマンドは変動する可能性がある
        public void UpdateRoutedCommand()
        {
            var oldies = Commands.Keys
                .Where(e => e.StartsWith(ScriptCommand.Prefix))
                .ToList();

            var newers = CommandTable.Current.Keys
                .Where(e => e.StartsWith(ScriptCommand.Prefix))
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

        // InputGesture設定
        public void InitializeInputGestures()
        {
            if (!_isDarty) return;
            _isDarty = false;

            UpdateRoutedCommand();

            // Touch
            var touch = TouchInput.Current;

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
                            command.Value.Execute(null, MainWindow.Current);
                            x.Handled = true;
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
                var mouseGesture = CommandTable.Current.GetElement(command.Key).MouseGesture;
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
                command.Execute(null, MainWindow.Current);
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
            int turn = Math.Abs(_mouseWheelDelta.NotchCount(arg));
            if (turn == 0) return;

            // Debug.WriteLine($"WheelCommand: {turn}({arg.Delta})");
            var param = new CommandParameterArgs(null, CommandTable.Current.IsReversePageMoveWheel);
            for (int i = 0; i < turn; i++)
            {
                command.Execute(param, MainWindow.Current);
            }
        }

        // コマンド実行 
        // CommandTableを純粋なコマンド定義のみにするため、コマンド実行に伴う処理はここで定義している
        public void Execute(string name, object parameter)
        {
            bool allowFlip = (parameter is CommandParameterArgs args)
                ? args.AllowFlip
                : (parameter != MenuCommandTag.Tag);

            var command = CommandTable.Current.GetElement(GetFixedCommandName(name, allowFlip));

            // 通知
            if (command.IsShowMessage)
            {
                string message = command.ExecuteMessage(CommandElement.EmptyArgs, CommandOption.None);
                if (message != null)
                {
                    InfoMessage.Current.SetMessage(InfoMessageType.Command, message);
                }
            }

            // 実行
            var option = (parameter is MenuCommandTag) ? CommandOption.ByMenu : CommandOption.None;
            command.Execute(CommandElement.EmptyArgs, option);
        }

        // スライダー方向によって移動コマンドを入れ替える
        public string GetFixedCommandName(string name, bool allowFlip)
        {
            if (allowFlip && CommandTable.Current.IsReversePageMove && MainWindowModel.Current.IsLeftToRightSlider())
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
