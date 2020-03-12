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
            set { _command.MouseGesture = value.Replace("←", "L").Replace("↑", "U").Replace("→", "R").Replace("↓", "L").Replace("Click","C"); }
        }

        public CommandParameter Parameter
        {
            get { return _command.Parameter; }
        }


        public bool Execute(params object[] args)
        {
            var param = _command.CreateOverwriteCommandParameter(_patch);
            if (_command.CanExecute(param, args, CommandOption.None))
            {
                _command.Execute(param, args, CommandOption.None);
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
