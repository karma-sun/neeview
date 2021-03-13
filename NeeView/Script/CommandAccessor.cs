using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// コマンドアクセス
    /// </summary>
    public class CommandAccessor
    {
        private object _sender;
        private CommandElement _command;
        private IDictionary<string, object> _patch;

        public CommandAccessor(object sender, CommandElement command)
        {
            _sender = sender;
            _command = command;
            Parameter = _command.Parameter != null ? new PropertyMap(_command.Parameter, $"nv.Command[{_command.Name}].Parameter") : null;
        }

        [WordNodeMember]
        public bool IsShowMessage
        {
            get { return _command.IsShowMessage; }
            set { _command.IsShowMessage = value; }
        }

        [WordNodeMember]
        public string ShortCutKey
        {
            get { return _command.ShortCutKey; }
            set { _command.ShortCutKey = value; }
        }

        [WordNodeMember]
        public string TouchGesture
        {
            get { return _command.TouchGesture; }
            set { _command.TouchGesture = value; }
        }

        [WordNodeMember]
        public string MouseGesture
        {
            get { return _command.MouseGesture; }
            set { _command.MouseGesture = value.Replace("←", "L").Replace("↑", "U").Replace("→", "R").Replace("↓", "L").Replace("Click", "C"); }
        }

        [WordNodeMember(IsAutoCollect = false)]
        public PropertyMap Parameter { get; }


        [WordNodeMember]
        public bool Execute(params object[] args)
        {
            var parameter = _command.CreateOverwriteCommandParameter(_patch);
            var context = new CommandContext(parameter, args, CommandOption.None);
            if (_command.CanExecute(_sender, context))
            {
                AppDispatcher.Invoke(() => _command.Execute(_sender, context));
                return true;
            }
            else
            {
                return false;
            }
        }

        [WordNodeMember]
        public CommandAccessor Patch(IDictionary<string, object> patch)
        {
            if (_patch == null)
            {
                _patch = patch;
            }
            else
            {
                foreach (var pair in patch)
                {
                    _patch[pair.Key] = pair.Value;
                }
            }

            return this;
        }

        internal WordNode CreateWordNode(string commandName)
        {
            var node = WordNodeHelper.CreateClassWordNode(commandName, this.GetType());

            if (Parameter != null)
            {
                node.Children.Add(Parameter.CreateWordNode(nameof(Parameter)));
            }

            return node;
        }

    }
}
