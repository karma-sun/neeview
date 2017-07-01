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

        public event EventHandler Changed;

        public event EventHandler<CommandExecutedEventArgs> CommandExecuted;

        //
        public Dictionary<CommandType, RoutedUICommand> Commands { get; set; } = new Dictionary<CommandType, RoutedUICommand>();

        // インテグザ
        public RoutedUICommand this[CommandType key]
        {
            get { return Commands[key]; }
            set { Commands[key] = value; }
        }

        //
        private MainWindow _window;
        private CommandTable _commandTable;

        //
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
            _commandTable.Changed +=
                (s, e) => InitializeInputGestures();
        }

        // InputGesture設定
        public void InitializeInputGestures()
        {
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
                        if (!x.TouchEventArgs.Handled && x.Gesture == gesture)
                        {
                            command.Value.Execute(null, _window);
                            x.TouchEventArgs.Handled = true;
                        }
                    };
                }
            }


            // Mouse / Keyboard
            var mouse = MouseInput.Current;

            mouse.ClearMouseEventHandler();

            MouseGestureCommandCollection.Current.Clear();

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
                        mouseNormalHandlers.Add((s, x) => MouseButtonCommandExecute(s, x, gesture, command.Value));
                    }
                    else if (gesture is MouseExGesture)
                    {
                        mouseExtraHndlers.Add((s, x) => MouseButtonCommandExecute(s, x, gesture, command.Value));
                        //mouseExtraHndlers.Add((s, x) => { if (!x.Handled && gesture.Matches(this, x)) { command.Value.Execute(null, _window); x.Handled = true; } });
                    }
                    else if (gesture is MouseWheelGesture)
                    {
                        mouse.MouseWheelChanged += (s, x) => { if (!x.Handled && gesture.Matches(this, x)) { WheelCommandExecute(command.Value, x); } };
                    }
                    else
                    {
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

            // 拡張マウス入力から先に処理を行う
            foreach (var lambda in mouseExtraHndlers.Concat(mouseNormalHandlers))
            {
                mouse.MouseButtonChanged += lambda;
            }

            //
            Changed?.Invoke(this, null);
        }


        //
        private void MouseButtonCommandExecute(object sender, MouseButtonEventArgs x, InputGesture gesture, RoutedUICommand command)
        {
            if (!x.Handled && gesture.Matches(this, x))
            {
                command.Execute(null, _window);
                CommandExecuted?.Invoke(this, new CommandExecutedEventArgs() { Gesture = gesture });
                x.Handled = true;

                /*
                if (gesture is MouseGesture mouse)
                {
                    Debug.WriteLine($"{command.Text}: {mouse.MouseAction}");
                }
                else if (gesture is MouseExGesture mouseEx)
                {
                    Debug.WriteLine($"{command.Text}: EX.{mouseEx.MouseExAction}");
                }
                */
            }
        }

        /// <summary>
        /// wheel command
        /// </summary>
        private void WheelCommandExecute(RoutedUICommand command, MouseWheelEventArgs arg)
        {
            int turn = MouseInputHelper.DeltaCount(arg);

            // Debug.WriteLine($"WheelCommand: {turn}({arg.Delta})");

            for (int i = 0; i < turn; i++)
            {
                command.Execute(null, _window);
            }
        }


        // コマンド実行 
        // CommandTableを純粋なコマンド定義のみにするため、コマンド実行に伴う処理はここで定義している
        public void Execute(CommandType type, object sender, object param)
        {
            var command = _commandTable[GetFixedCommandType(type)];

            // 通知
            if (command.IsShowMessage)
            {
                string message = command.ExecuteMessage(param);
                InfoMessage.Current.SetMessage(InfoMessageType.Command, message);
            }

            // 実行
            command.Execute(sender, param);
        }

        // ペアコマンドとの交換
        public CommandType GetFixedCommandType(CommandType commandType)
        {
            // TODO: ホイール操作も逆転してしまい、使用に問題があるため保留
#if false
            if (CommandTable.Current.IsReversePageMoveGesture && BookSetting.Current.BookMemento.BookReadOrder == PageReadOrder.LeftToRight)
            {
                var command = _commandTable[commandType];
                if (command.PairPartner != CommandType.None)
                {
                    Debug.WriteLine($"SwapCommand: {commandType} to {command.PairPartner}");
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
#else
            return commandType;
#endif
        }

        //
        public CommandElement GetFixedCommandElement(CommandType commandType)
        {
            _commandTable.TryGetValue(GetFixedCommandType(commandType), out CommandElement command);
            return command;
        }

        //
        public RoutedUICommand GetFixedRoutedCommand(CommandType commandType)
        {
            this.Commands.TryGetValue(GetFixedCommandType(commandType), out RoutedUICommand command);
            return command;
        }


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
}
