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
    /// コマンド集 ： RoutedCommand
    /// </summary>
    public class RoutedCommandTable
    {
        public static RoutedCommandTable Current { get; private set; }

        public event EventHandler Changed;

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

            mouse.CommandCollection.Clear();
            mouse.MouseGestureChanged += (s, x) => mouse.CommandCollection.Execute(x.Sequence);

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
                        mouseNormalHandlers.Add((s, x) => { if (!x.Handled && x.StylusDevice == null && gesture.Matches(this, x)) { command.Value.Execute(null, _window); x.Handled = true; } });
                    }
                    else if (gesture is MouseExGesture)
                    {
                        mouseExtraHndlers.Add((s, x) => { if (!x.Handled && gesture.Matches(this, x)) { command.Value.Execute(null, _window); x.Handled = true; } });
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
                    mouse.CommandCollection.Add(mouseGesture, command.Value);
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
            // 通知
            if (_commandTable[type].IsShowMessage)
            {
                string message = _commandTable[type].ExecuteMessage(param);
                InfoMessage.Current.SetMessage(InfoMessageType.Command, message);
            }

            // 実行
            _commandTable[type].Execute(sender, param);
        }


        #region Memento
        // compatible before ver.23
        [Obsolete, DataContract]
        public class Memento
        {
            [Obsolete, DataMember]
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
