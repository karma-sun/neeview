using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// コントロールにコマンドをバインドする
    /// </summary>
    class RoutedCommandBinding
    {
        private FrameworkElement _element;
        private Dictionary<string, CommandBinding> _commandBindings;
        private bool _skipMouseButtonUp;
        private RoutedCommandTable _routedCommandTable;

        public RoutedCommandBinding(FrameworkElement element, RoutedCommandTable routedCommandTable)
        {
            _element = element;
            _element.PreviewMouseUp += Control_PreviewMouseUp;
            _element.PreviewKeyDown += Control_PreviewKeyDown;

            _routedCommandTable = routedCommandTable;
            _routedCommandTable.CommandExecuted += RoutedCommand_CommandExecuted;

            InitializeCommandBindings();
        }


        private void InitializeCommandBindings()
        {
            _commandBindings = new Dictionary<string, CommandBinding>();

            foreach (var name in CommandTable.Current.Keys)
            {
                var binding = CreateCommandBinding(name);
                _commandBindings.Add(name, binding);
                _element.CommandBindings.Add(binding);
            }

            _routedCommandTable.Changed += RefresuCommandBindings;
        }

        // コマンド実行後処理
        private void RoutedCommand_CommandExecuted(object sender, CommandExecutedEventArgs e)
        {
            // ダブルクリックでコマンド実行後のMouseButtonUpイベントをキャンセルする
            if (e.Gesture is MouseGesture mouse)
            {
                switch (mouse.MouseAction)
                {
                    case MouseAction.LeftDoubleClick:
                    case MouseAction.RightDoubleClick:
                    case MouseAction.MiddleDoubleClick:
                        _skipMouseButtonUp = true;
                        break;
                }
            }
            else if (e.Gesture is MouseExGesture mouseEx)
            {
                switch (mouseEx.MouseExAction)
                {
                    case MouseExAction.LeftDoubleClick:
                    case MouseExAction.RightDoubleClick:
                    case MouseExAction.MiddleDoubleClick:
                    case MouseExAction.XButton1DoubleClick:
                    case MouseExAction.XButton2DoubleClick:
                        _skipMouseButtonUp = true;
                        break;
                }
            }
        }

        private CommandBinding CreateCommandBinding(string commandName)
        {
            var binding = new CommandBinding(_routedCommandTable.Commands[commandName],
                (sender, e) => _routedCommandTable.Execute(sender, commandName, e.Parameter),
                (sender, e) => e.CanExecute = CommandTable.Current.GetElement(commandName).CanExecute(sender, CommandArgs.Empty));

            return binding;
        }

        private void RefresuCommandBindings(object sender, EventArgs _)
        {
            var oldies = _commandBindings.Keys
                .Where(e => e.StartsWith(ScriptCommand.Prefix))
                .ToList();

            var newers = CommandTable.Current.Keys
                .Where(e => e.StartsWith(ScriptCommand.Prefix))
                .ToList();

            foreach (var name in oldies.Except(newers))
            {
                var binding = _commandBindings[name];
                _commandBindings.Remove(name);
                _element.CommandBindings.Remove(binding);
            }

            foreach (var name in newers.Except(oldies))
            {
                var binding = CreateCommandBinding(name);
                _commandBindings.Add(name, binding);
                _element.CommandBindings.Add(binding);
            }
        }

        private void Control_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // ダブルクリック後のイベントキャンセル
            if (_skipMouseButtonUp)
            {
                ///Debug.WriteLine("Skip MuseUpEvent");
                _skipMouseButtonUp = false;
                e.Handled = true;
            }
        }

        private void Control_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 単キーのショートカットを有効にする。
            // TextBoxなどのイベント処理でこのフラグをfalseにすることで短キーのショートカットを無効にして入力を優先させる
            KeyExGesture.AllowSingleKey = true;

            // 一部 IMEKey のっとり
            if (e.Key == Key.ImeProcessed && e.ImeProcessedKey.IsImeKey())
            {
                RoutedCommandTable.Current.ExecuteImeKeyGestureCommand(sender, e);
            }
        }
    }
}
