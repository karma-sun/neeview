using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// コマンドアクセス
    /// </summary>
    public class CommandAccessor
    {
        private CommandElement _command;
        private IDictionary<string, object> _patch;

        public CommandAccessor(CommandElement command)
        {
            _command = command;
            Parameter = _command.Parameter != null ? new PropertyMap(_command.Parameter) : null;
        }

        public string ShortCutKey
        {
            get { return _command.ShortCutKey; }
            set { _command.ShortCutKey = value; }
        }

        public string TouchGesture
        {
            get { return _command.TouchGesture; }
            set { _command.TouchGesture = value; }
        }

        public string MouseGesture
        {
            get { return _command.MouseGesture; }
            set { _command.MouseGesture = value.Replace("←", "L").Replace("↑", "U").Replace("→", "R").Replace("↓", "L").Replace("Click", "C"); }
        }

        public PropertyMap Parameter { get; }


        public bool Execute(params object[] args)
        {
            var parameter = _command.CreateOverwriteCommandParameter(_patch);
            var arguments = args ?? CommandElement.EmptyArgs;
            if (_command.CanExecute(parameter, arguments, CommandOption.None))
            {
                _command.Execute(parameter, arguments, CommandOption.None);
                return true;
            }
            else
            {
                return false;
            }
        }

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
    }

}
